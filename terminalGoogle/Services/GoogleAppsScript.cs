using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Utilities.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Requests;
using Google.Apis.Script.v1;
using Google.Apis.Script.v1.Data;
using Google.Apis.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Interfaces;
using terminalGoogle.Services.Authorization;

namespace terminalGoogle.Services
{
    public class GoogleAppsScript : IGoogleAppsScript 
    {
        readonly GoogleAuthorizer _googleAuth;
        private readonly IGoogleDrive _googleDrive;

        public GoogleAppsScript(IGoogleDrive googleDrive)
        {
            _googleAuth = new GoogleAuthorizer();
            _googleDrive = googleDrive;
        }

        public async Task<List<GoogleFormField>> GetGoogleFormFields(GoogleAuthDTO authDTO, string formId)
        {
            try
            {
                //allow edit permissions to our main google account on current form in Google Drive 
                await AllowPermissionsForGoogleDriveFile(authDTO, formId);

                // run apps script deployed as web app to return form fields as json object
                _googleAuth.CreateFlowMetadata(authDTO, "", CloudConfigurationManager.GetSetting("GoogleRedirectUri"));

                var appScriptUrl = CloudConfigurationManager.GetSetting("GoogleAppScriptWebApp");

                appScriptUrl = string.Format(appScriptUrl + "?formId={0}&action={1}", formId, "getFormFields");

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    authDTO.AccessToken);

                //check received response
                var responseMessage = await client.GetAsync(new Uri(appScriptUrl));
                var contents = await responseMessage.Content.ReadAsStringAsync();

                var googleForm = new GoogleForm();
                JsonConvert.PopulateObject(contents, googleForm);

                var result = new List<GoogleFormField>();
                foreach (var item in googleForm.FormFields)
                {
                    result.Add(new GoogleFormField()
                    {
                        Id = item.Id,
                        Title = item.Title,
                        Index = item.Index,
                        Type =  item.Type,
                    });
                }

                return await Task.FromResult(result);
            }
            catch (Exception e)
            {
                //in case of error generate script link for user
                throw new Exception(e.Message);
            }
        }

        public async Task CreateFr8TriggerForDocument(GoogleAuthDTO authDTO, string formId, string email)
        {
            try
            {
                //allow edit permissions to our main google account on current form in Google Drive 
                await AllowPermissionsForGoogleDriveFile(authDTO, formId);
                
                // run apps script deployed as web app to create trigger for this form
                _googleAuth.CreateFlowMetadata(authDTO, "", CloudConfigurationManager.GetSetting("GoogleRedirectUri"));

                var appScriptUrl = CloudConfigurationManager.GetSetting("GoogleAppScriptWebApp");
                var formEventWebServerUrl = CloudConfigurationManager.GetSetting("GoogleFormEventWebServerUrl");

                appScriptUrl = string.Format(appScriptUrl + "?formId={0}&endpoint={1}&email={2}", formId, formEventWebServerUrl, email);

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    authDTO.AccessToken);

                var responseMessage = await client.GetAsync(new Uri(appScriptUrl));
                await responseMessage.Content.ReadAsStringAsync();
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
                var driveService = await _googleDrive.CreateDriveService(authDTO);

                var formFilename = FormFilename(driveService, formId);

                Google.Apis.Drive.v3.Data.File scriptFile = new Google.Apis.Drive.v3.Data.File();
                scriptFile.Name = "Script for: " + formFilename;
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
                    if (!_googleDrive.FileExist(driveService, scriptFile.Name, out existingFileLink))
                    {
                        FilesResource.CreateMediaUpload request = driveService.Files.Create(scriptFile, memoryStream, "application/vnd.google-apps.script+json");
                        request.Upload();
                        response = request.ResponseBody.WebViewLink;
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


        private string FormFilename(DriveService driveService, string id)
        {
            string fileName = "";
            // Define parameters of request.
            FilesResource.ListRequest listRequest = driveService.Files.List();
            listRequest.Q = "mimeType='application/vnd.google-apps.form'  and Trashed=false";

            // List files.
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;

            foreach (var item in files)
            {
                if (item.Id == id)
                {
                    fileName = item.Name;
                    break;
                }
            }

            return fileName;
        }

        private async Task AllowPermissionsForGoogleDriveFile(GoogleAuthDTO authDTO, string fileId)
        {
            //create google drive service for file manipulation
            var driveService = await _googleDrive.CreateDriveService(authDTO);

            var batch = new BatchRequest(driveService);
            //bach service callback for successfull permission set
            BatchRequest.OnResponse<Permission> callback = delegate (
                Permission permission, RequestError error, int index, HttpResponseMessage message){
                if (error != null)
                {
                    // Handle error
                    throw new ApplicationException($"Problem with Google Drive Permissions: {error.Message}");
                }
            };

            var userPermission = new Permission
            {
                Type = "user",
                Role = "writer",
                EmailAddress = CloudConfigurationManager.GetSetting("GoogleMailAccount")
            };
            var request = driveService.Permissions.Create(userPermission, fileId);
            request.Fields = "id";
            batch.Queue(request, callback);

            await batch.ExecuteAsync();
        }
    }
}