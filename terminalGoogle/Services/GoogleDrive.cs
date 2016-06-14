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
using Fr8.Infrastructure.Utilities.Configuration;
using Google.Apis.Requests;
using Google.Apis.Script.v1;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Interfaces;
using terminalGoogle.Services.Authorization;

namespace terminalGoogle.Services
{
    public class GoogleDrive : IGoogleDrive
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

        public async Task DeleteForm(string formId, GoogleAuthDTO authDTO)
        {
            var driveService = await CreateDriveService(authDTO);
            driveService.Files.Delete(formId).Execute();
        }
    }
}