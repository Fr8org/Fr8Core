using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Google.Apis.Requests;
using Google.Apis.Script.v1;
using Google.Apis.Script.v1.Data;
using Hub.Managers;
using terminalGoogle.DataTransferObjects;
using TerminalBase;
using Utilities.Configuration.Azure;

namespace terminalGoogle.Services
{
    public class GoogleDrive
    {
        readonly GoogleAuthorizer _googleAuth;

        public GoogleDrive()
        {
            _googleAuth = new GoogleAuthorizer();
        }

        public async Task<DriveService> CreateDriveService(GoogleAuthDTO authDTO)
        {
            var flowData = _googleAuth.CreateFlowMetadata(authDTO, "", CloudConfigurationManager.GetSetting("GoogleRedirectUri"));
            TokenResponse tokenResponse = new TokenResponse();
            tokenResponse.AccessToken = authDTO.AccessToken;
            tokenResponse.RefreshToken = authDTO.RefreshToken;
            tokenResponse.Scope = CloudConfigurationManager.GetSetting("GoogleScope");

            UserCredential userCredential;
            try
            {
                userCredential = new UserCredential(flowData.Flow, authDTO.AccessToken, tokenResponse);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            // Create Drive API service.
            DriveService driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredential,
                ApplicationName = "Fr8",
            });

            return driveService;
        }

        public async Task<string> DownloadFile(string fileId, GoogleAuthDTO authDTO)
        {
            var driveService = await CreateDriveService(authDTO);
            var getRequest = driveService.Files.Get(fileId);
            var file = await getRequest.ExecuteAsync();
            var downloadUlr = file.DownloadUrl;
            string fileContent;

            using (var httpClient = new HttpClient())
            {
                fileContent = await httpClient.GetStringAsync(downloadUlr);
            }

            return fileContent;
        }

        public async Task<Dictionary<string, string>> GetGoogleForms(GoogleAuthDTO authDTO)
        {
            var driveService = await CreateDriveService(authDTO);
            
            // Define parameters of request.
            FilesResource.ListRequest listRequest = driveService.Files.List();
            listRequest.Q = "mimeType='application/vnd.google-apps.form'  and Trashed=false";

            // List files.
            FileList fileList = listRequest.Execute();
            IList<Google.Apis.Drive.v2.Data.File> files = fileList.Items;
            return await Task.FromResult(files.ToDictionary(a => a.Id, a => a.Title));
        }

        public async Task<string> CreateGoogleForm(GoogleAuthDTO authDTO, string title)
        {
            var driveService = await CreateDriveService(authDTO);
            var file = new Google.Apis.Drive.v2.Data.File();
            file.Title = title;
            file.MimeType = "application/vnd.google-apps.form";
            var request = driveService.Files.Insert(file);
            var result = request.Execute();
            return result.Id;
        }

        public async Task<ScriptService> CreateScriptsService(GoogleAuthDTO authDTO)
        {
            var flowData = _googleAuth.CreateFlowMetadata(authDTO, "", CloudConfigurationManager.GetSetting("GoogleRedirectUri"));
            TokenResponse tokenResponse = new TokenResponse();
            tokenResponse.AccessToken = authDTO.AccessToken;
            tokenResponse.RefreshToken = authDTO.RefreshToken;
            tokenResponse.Scope = CloudConfigurationManager.GetSetting("GoogleScope");

            UserCredential userCredential;
            try
            {
                userCredential = new UserCredential(flowData.Flow, authDTO.AccessToken, tokenResponse);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            var scriptsService = new ScriptService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredential,
                ApplicationName = "Fr8",
            });

            return scriptsService;
        }
        
        public async Task<bool> CreateFr8TriggerForDocument(GoogleAuthDTO authDTO, string formId, string email)
        {
            try
            {
                var driveService = await CreateDriveService(authDTO);

                //allow edit permissions to our main google account on current form in Google Drive 
                var batch = new BatchRequest(driveService);
                BatchRequest.OnResponse<Permission> callback = delegate (
                    Permission permission,
                    RequestError error,
                    int index,
                    HttpResponseMessage message)
                {
                    if (error != null)
                    {
                        // Handle error
                        throw new ApplicationException($"Problem with Google Drive Permission on creating Fr8Trigger: {error.Message} ");
                    }
                };
                Permission userPermission = new Permission();
                userPermission.Type = "user";
                userPermission.Role = "writer";
                userPermission.Value = CloudConfigurationManager.GetSetting("GoogleMailAccount");
                var request = driveService.Permissions.Insert(userPermission, formId);
                request.Fields = "id";
                batch.Queue(request, callback);

                await batch.ExecuteAsync();

                // run apps script deployed as web app to create trigger for this form
                var flowData = _googleAuth.CreateFlowMetadata(authDTO, "", CloudConfigurationManager.GetSetting("GoogleRedirectUri"));

                var appScriptUrl = CloudConfigurationManager.GetSetting("GoogleAppScriptWebApp");
                var formEventWebServerUrl = CloudConfigurationManager.GetSetting("GoogleFormEventWebServerUrl");

                appScriptUrl = string.Format(appScriptUrl + "?formId={0}&endpoint={1}&email={2}", formId, formEventWebServerUrl, email);

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    authDTO.AccessToken);

                //check received response
                var responseMessage = await client.GetAsync(new Uri(appScriptUrl));
                var contents = await responseMessage.Content.ReadAsStringAsync();

                return contents == "OK" ? await Task.FromResult(true) : await Task.FromResult(false);
            }
            catch (Exception e)
            {
                //in case of error generate script link for user
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// This is manual approach for creating Fr8 trigger for some document, mainly used as backup plan for automatic apps script run
        /// </summary>
        /// <param name="authDTO"></param>
        /// <param name="formId"></param>
        /// <param name="desription"></param>
        /// <returns></returns>
        public async Task<string> CreateManualFr8TriggerForDocument(GoogleAuthDTO authDTO, string formId, string desription = "Script uploaded from Fr8 application")
        {
            string response = "";
            try
            {
                var driveService = await CreateDriveService(authDTO);

                var formFilename = FormFilename(driveService, formId);

                Google.Apis.Drive.v2.Data.File scriptFile = new Google.Apis.Drive.v2.Data.File();
                scriptFile.Title = "Script for: " + formFilename;
                scriptFile.Description = desription;
                scriptFile.MimeType = "application/vnd.google-apps.script+json";

                // Create a memory stream
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    //load template file and replace specific formID
                    var assembly = Assembly.GetExecutingAssembly();
                    var resourceName = "terminalGoogle.Template.googleAppScriptFormResponse.json";
                    string content;

                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        content = reader.ReadToEnd();
                    }

                    content = content.Replace("@ID", formId);
                    content = content.Replace("@ENDPOINT", CloudConfigurationManager.GetSetting("GoogleFormEventWebServerUrl"));
                    byte[] contentAsBytes = Encoding.UTF8.GetBytes(content);
                    memoryStream.Write(contentAsBytes, 0, contentAsBytes.Length);

                    // Set the position to the beginning of the stream.
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    //upload file to google drive
                    string existingFileLink = "";
                    if (!FileExist(driveService, scriptFile.Title, out existingFileLink))
                    {
                        FilesResource.InsertMediaUpload request = driveService.Files.Insert(scriptFile, memoryStream, "application/vnd.google-apps.script+json");
                        request.Upload();
                        response = request.ResponseBody.AlternateLink;
                    }
                    else
                        response = existingFileLink;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return await Task.FromResult(response);
        }


        public bool FileExist(DriveService driveService, string filename, out string link)
        {
            bool exist = false;

            // Define parameters of request.
            FilesResource.ListRequest listRequest = driveService.Files.List();
            listRequest.Q = string.Format("title='{0}' and Trashed=false", filename);

            // List files.
            IList<Google.Apis.Drive.v2.Data.File> files = listRequest.Execute()
                .Items;

            if (files.Count > 0)
            {
                exist = true;
                link = files[0].AlternateLink;
            }
            else
            {
                exist = false;
                link = "";
            }

            return exist;
        }

        public string FormFilename(DriveService driveService, string id)
        {
            string fileName = "";
            // Define parameters of request.
            FilesResource.ListRequest listRequest = driveService.Files.List();
            listRequest.Q = "mimeType='application/vnd.google-apps.form'  and Trashed=false";

            // List files.
            IList<Google.Apis.Drive.v2.Data.File> files = listRequest.Execute()
                .Items;

            foreach (var item in files)
            {
                if (item.Id == id)
                {
                    fileName = item.Title;
                    break;
                }
            }

            return fileName;
        }
        public async Task DeleteForm(string formId, GoogleAuthDTO authDTO)
        {
            GoogleDrive googleDrive = new GoogleDrive();
            var driveService = await googleDrive.CreateDriveService(authDTO);
            driveService.Files.Delete(formId).Execute();
        }
    }
}