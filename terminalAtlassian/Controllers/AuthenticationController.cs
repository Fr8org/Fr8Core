using System.Web.Http;
using TerminalBase.BaseClasses;
using Newtonsoft.Json;
using terminalAtlassian.Services;
using StructureMap;
using System;
using Fr8Data.DataTransferObjects;

namespace terminalAtlassian.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : BaseTerminalController
    {
        private readonly AtlassianService _atlassianService;
        private const string curTerminal = "terminalAtlassian";

        public AuthenticationController()
        {
            _atlassianService = ObjectFactory.GetInstance<AtlassianService>();
        }

        [HttpPost]
        [Route("internal")]
        public  AuthorizationTokenDTO GenerateInternalOAuthToken(CredentialsDTO curCredential)
        {
            try
            {
                if (_atlassianService.IsValidUser(curCredential))
                {
                    return new AuthorizationTokenDTO()
                    {
                        Token = JsonConvert.SerializeObject(curCredential),
                        ExternalAccountId = curCredential.Username
                    };
                }
                return new AuthorizationTokenDTO()
                {
                    Error = "Unable to authenticate in Atlassian service, invalid domain,login name or password."
                };
            }
            catch (Exception ex)
            {
                ReportTerminalError(curTerminal, ex,curCredential.Fr8UserId);

                return new AuthorizationTokenDTO()
                {
                    Error = "An error occurred while trying to authorize, please try again later."
                };
            }

        }
    }
}