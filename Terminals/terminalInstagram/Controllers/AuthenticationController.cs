using System;
using System.Threading.Tasks;
using System.Web.Http;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Services;
using terminalInstagram.Interfaces;
using InstaSharp;
using terminalInstagram.Infrastructure;
using InstaSharp.Endpoints;

namespace terminalInstagram.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : ApiController
    {
        private readonly IInstagramIntegration _instagramIntegration;
        private readonly IHubEventReporter _eventReporter;

        public AuthenticationController(IInstagramIntegration instagramIntegration, IHubEventReporter eventReporter)
        {
            _instagramIntegration = instagramIntegration;
            _eventReporter = eventReporter;
        }

        [HttpPost]
        [Route("request_url")]
        public ExternalAuthUrlDTO GenerateOAuthInitiationURL()
        {
            var externalStateToken = Guid.NewGuid().ToString();
            var url = _instagramIntegration.CreateAuthUrl(externalStateToken);

            var externalAuthUrlDTO = new ExternalAuthUrlDTO()
            {
                ExternalStateToken = externalStateToken,
                Url = url
            };

            return externalAuthUrlDTO;
        }
        //[HttpPost]
        //[Route("token")]
        //public async Task<AuthorizationTokenDTO> GenerateOAuthToken(ExternalAuthenticationDTO externalAuthDTO)
        //{
        //    var authentication = new Authentication();
        //    try
        //    {
        //        var oauthToken = await GetOAuthToken(code);
        //        return new AuthorizationTokenDTO
        //        {
        //            Token = oauthToken
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        await _eventReporter.ReportTerminalError(ex, externalAuthDTO.Fr8UserId);

        //        return new AuthorizationTokenDTO()
        //        {
        //            Error = "An error occurred while trying to authorize, please try again later."
        //        };
        //    }
        //}
        //public async Task<string> GetOAuthToken(string code)
        //{
        //    var authentication = new Authentication();
        //    var config = authentication.createInstagramConfig();
        //    var auth = new OAuth(config);

        //    var oauthResponse = await auth.RequestToken(code);
        //    return oauthResponse.AccessToken;
        //}
    }
}