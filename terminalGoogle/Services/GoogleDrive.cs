using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Google.GData.Client;
using StructureMap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Interfaces;
using Utilities.Configuration.Azure;

namespace terminalGoogle.Services
{
    public class GoogleAppScript
    {
        private readonly IGoogleIntegration _googleIntegration;
        GoogleAuthorizer _googleAuth;

        /*public async Task<ScriptService> CreateScriptService(GoogleAuthDTO authDTO)
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
        }*/
    }


    public class GoogleDrive
    {
        private readonly IGoogleIntegration _googleIntegration;
        GoogleAuthorizer _googleAuth;
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

        public async Task<string> UploadAppScript(GoogleAuthDTO authDTO, string formId, string desription = "Script uploaded from Fr8 application")
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
                    string filename = System.Web.Hosting.HostingEnvironment.MapPath("~\\Template\\googleAppScriptFormResponse.json");
                    string content = System.IO.File.ReadAllText(filename);
                    content = content.Replace("@ID", formId);
                    content = content.Replace("@ENDPOINT", CloudConfigurationManager.GetSetting("GoogleFormEventWebServerUrl"));
                    byte[] contentAsBytes = Encoding.UTF8.GetBytes(content);
                    memoryStream.Write(contentAsBytes, 0, contentAsBytes.Length);

                    // Set the position to the beginning of the stream.
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    //upload file to google drive
                    string existingFileId = "";
                    if (!FileExist(driveService, scriptFile.Title, out existingFileId))
                    {
                        FilesResource.InsertMediaUpload request = driveService.Files.Insert(scriptFile, memoryStream, "application/vnd.google-apps.script+json");
                        request.Upload();
                        response = request.ResponseBody.Id;
                    }
                    else
                        response = existingFileId;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return await Task.FromResult(response);
        }

        public bool FileExist(DriveService driveService, string filename, out string fileId)
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
                fileId = files[0].Id;
            }
            else
            {
                exist = false;
                fileId = "";
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

    }
}