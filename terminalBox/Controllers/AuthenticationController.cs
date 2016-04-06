using System;
using System.Threading.Tasks;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using TerminalBase.BaseClasses;
using Utilities.Configuration.Azure;

namespace terminalBox.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : BaseTerminalController
    {
        private const string curTerminal = "terminalBox";
        //https://account.box.com/api/oauth2/authorize?response_type=code&client_id=MY_CLIENT_ID&state=security_token%3DKnhMJatFipTAnM0nHlZA
        //http://localhost:30643/AuthenticationCallback/ProcessSuccessfulOAuthResponse
        [HttpPost]
        [Route("initial_url")]
        public ExternalAuthUrlDTO GenerateOAuthInitiationURL()
        {
            var url = CloudConfigurationManager.GetSetting("BoxAuthUrl");
            var clientId = CloudConfigurationManager.GetSetting("BoxClientId");
            var redirectUri = CloudConfigurationManager.GetSetting("BoxCallbackUrlsDomain") + "AuthenticationCallback/ProcessSuccessfulOAuthResponse";
            var state = Guid.NewGuid().ToString();

            url = url + string.Format("authorize?response_type=code&client_id={0}&redirect_uri={1}&state={2}",
                clientId, System.Web.HttpUtility.UrlEncode(redirectUri),
               System.Web.HttpUtility.UrlEncode(state));

            return new ExternalAuthUrlDTO() { Url = url, ExternalStateToken = System.Web.HttpUtility.UrlEncode(state) };
        }

        //[HttpPost]
        //[Route("token")]
        //public async Task<AuthorizationTokenDTO> GenerateOAuthToken(
        // ExternalAuthenticationDTO externalAuthDTO)
        //{
        //    try
        //    {
        //        return await _authentication.Authenticate(externalAuthDTO);
        //    }
        //    catch (Exception ex)
        //    {
        //        ReportTerminalError(curTerminal, ex);
        //        return await Task.FromResult(
        //            new AuthorizationTokenDTO()
        //            {
        //                Error = "An error occurred while trying to authorize, please try again later."
        //            }
        //        );
        //    }
        //}
    }
}