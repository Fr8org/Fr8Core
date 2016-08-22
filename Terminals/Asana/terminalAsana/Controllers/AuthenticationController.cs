using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Services;
using Newtonsoft.Json.Linq;
using terminalAsana.Interfaces;

namespace terminalAsana.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : ApiController
    {
        private readonly IAsanaOAuth _asanaOAuth;
        private readonly IHubLoggerService _loggerService;

        public AuthenticationController(IAsanaOAuth asanaOAuth, IHubLoggerService loggerService)
        {
            //we don`t need whole client here, so i can use only AsanaOAuth service
            _asanaOAuth = asanaOAuth;
            _loggerService = loggerService;
        }

        [HttpPost]
        [Route("request_url")]
        public ExternalAuthUrlDTO GenerateOAuthInitiationURL()
        {
            var externalStateToken = Guid.NewGuid().ToString();
            var url = _asanaOAuth.CreateAuthUrl(externalStateToken);

            var externalAuthUrlDTO = new ExternalAuthUrlDTO()
            {
                ExternalStateToken = externalStateToken,
                Url = url
            };

            return externalAuthUrlDTO;
        }

        [HttpPost]
        [Route("token")]
        public async Task<AuthorizationTokenDTO> GenerateOAuthToken(ExternalAuthenticationDTO externalAuthDTO)
        {
            try
            {

                var query = HttpUtility.ParseQueryString(externalAuthDTO.RequestQueryString);
                string code = query["code"];
                string state = query["state"];

                if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
                {
                    throw new ApplicationException("Code or State is empty.");
                }

                var oauthTokenData = await _asanaOAuth.GetOAuthTokenDataAsync(code);
                var userInfo = oauthTokenData.Value<JObject>("data");
                var secondsToExpiration = oauthTokenData.Value<int>("expires_in");
                var expirationDate = _asanaOAuth.CalculateExpirationTime(secondsToExpiration);

                return new AuthorizationTokenDTO
                {
                    Token = oauthTokenData.ToString(),
                    ExternalAccountId = userInfo.Value<string>("id"),
                    ExternalAccountName = userInfo.Value<string>("name"),
                    ExternalStateToken = state,
                    ExpiresAt = expirationDate,
                    AdditionalAttributes = expirationDate.ToString("O")
                };
            }
            catch (Exception ex)
            {
                await _loggerService.ReportTerminalError(ex, externalAuthDTO.Fr8UserId);

                return new AuthorizationTokenDTO()
                {
                    Error = "An error occurred while trying to authorize, please try again later."
                };
            }
        }
    }
}