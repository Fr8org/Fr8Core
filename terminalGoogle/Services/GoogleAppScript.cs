using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Utilities.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Script.v1;
using Google.Apis.Script.v1.Data;
using Google.Apis.Services;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Services.Authorization;

namespace terminalGoogle.Services
{
    public class GoogleAppScript
    {
        readonly GoogleAuthorizer _googleAuth;
        
        public GoogleAppScript()
        {
            _googleAuth = new GoogleAuthorizer();
        }

        private ScriptService CreateScriptService(GoogleAuthDTO authDTO)
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

            // Create Script API service.
            ScriptService driveService = new ScriptService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredential,
                ApplicationName = "Fr8",
            });

            return driveService;
        }
        
        public async Task<IDictionary<string, object>> RunScript(string scriptId, string functionName, GoogleAuthDTO authDto, params object[] parameters)
        {
            var service = CreateScriptService(authDto);

            // Create an execution request object.
            ExecutionRequest request = new ExecutionRequest
            {
                Function = functionName,
                Parameters = parameters?.Length > 0 ? new List<object>(parameters) : null
            };

            var runReq = service.Scripts.Run(request, scriptId);
            
            // Make the API request.
            Operation op = await runReq.ExecuteAsync();

            if (op.Error != null)
            {
                // The API executed, but the script returned an error.

                // Extract the first (and only) set of error details
                // as a IDictionary. The values of this dictionary are
                // the script's 'errorMessage' and 'errorType', and an
                // array of stack trace elements. Casting the array as
                // a JSON JArray allows the trace elements to be accessed
                // directly.
                var error = op.Error.Details[0];
                var errorDetails = new StringBuilder();

                errorDetails.AppendLine("Script error message: " + error["errorMessage"]);


                if (error["scriptStackTraceElements"] != null)
                {
                    // There may not be a stacktrace if the script didn't
                    // start executing.
                    errorDetails.AppendLine("Script error stacktrace:");
                    var st = (Newtonsoft.Json.Linq.JArray) error["scriptStackTraceElements"];
                    foreach (var trace in st)
                    {
                        errorDetails.AppendFormat("\t{0}: {1}\n", trace["function"], trace["lineNumber"]);
                    }
                }

                throw new Exception(errorDetails.ToString());
            }
            // The result provided by the API needs to be cast into
            // the correct type, based upon what types the Apps
            // Script function returns. Here, the function returns
            // an Apps Script Object with String keys and values.
            // It is most convenient to cast the return value as a JSON
            // JObject (folderSet).
            return op.Response;
        }
    }
}