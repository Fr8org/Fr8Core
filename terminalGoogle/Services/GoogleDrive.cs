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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Script.v1;
using Google.Apis.Script.v1.Data;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Services.Authorization;
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
        /*
        public async Task CreateFr8TriggerForDocument(GoogleAuthDTO authDTO, string formId)
        {
            var fr8TriggerScriptId = CloudConfigurationManager.GetSetting("Fr8TriggerScript");
            var scriptsService = await CreateScriptsService(authDTO);

            //let's run our initialize function from the script
            ExecutionRequest executionRequest = new ExecutionRequest { Function = "CreateTrigger", Parameters = new List<object> { formId } };
            ScriptsResource.RunRequest runReq = scriptsService.Scripts.Run(executionRequest, fr8TriggerScriptId);
            try
            {
                // Make the API request.
                Operation op = runReq.Execute();

                if (op.Error != null)
                {
                    // The API executed, but the script returned an error.
                    // Extract the first (and only) set of error details
                    // as a IDictionary. The values of this dictionary are
                    // the script's 'errorMessage' and 'errorType', and an
                    // array of stack trace elements. Casting the array as
                    // a JSON JArray allows the trace elements to be accessed
                    // directly.
                    IDictionary<string, object> error = op.Error.Details[0];
                    Console.WriteLine("Script error message: {0}", error["errorMessage"]);
                    if (error["scriptStackTraceElements"] != null)
                    {
                        // There may not be a stacktrace if the script didn't
                        // start executing.
                        Console.WriteLine("Script error stacktrace:");
                        Newtonsoft.Json.Linq.JArray st =
                            (Newtonsoft.Json.Linq.JArray)error["scriptStackTraceElements"];
                        foreach (var trace in st)
                        {
                            Console.WriteLine(
                                    "\t{0}: {1}",
                                    trace["function"],
                                    trace["lineNumber"]);
                        }
                    }
                }
            }
            catch (Google.GoogleApiException e)
            {
                // The API encountered a problem before the script
                // started executing.
                Console.WriteLine("Error calling API:\n{0}", e);
            }
        }
        */
        
        public async Task<string> CreateFr8TriggerForDocument(GoogleAuthDTO authDTO, string formId, string desription = "Script uploaded from Fr8 application")
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