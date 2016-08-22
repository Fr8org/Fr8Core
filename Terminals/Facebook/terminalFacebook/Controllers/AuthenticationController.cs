using System;
using System.Threading.Tasks;
using System.Web.Http;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Services;
using terminalFacebook.Interfaces;

namespace terminalFacebook.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : ApiController
    {
        private readonly IFacebookIntegration _fbIntegration;
        private readonly IHubLoggerService _loggerService;

        public AuthenticationController(IFacebookIntegration fbIntegration, IHubLoggerService loggerService)
        {
            _loggerService = loggerService;
            _fbIntegration = fbIntegration;
        }

        [HttpPost]
        [Route("request_url")]
        public ExternalAuthUrlDTO GenerateOAuthInitiationURL()
        {
            var externalStateToken = Guid.NewGuid().ToString();
            var url = _fbIntegration.CreateAuthUrl(externalStateToken);

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
                string code;
                string state;

                ParseCodeAndState(externalAuthDTO.RequestQueryString, out code, out state);

                if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
                {
                    throw new ApplicationException("Code or State is empty.");
                }

                var oauthToken = await _fbIntegration.GetOAuthToken(code);
                var userInfo = await _fbIntegration.GetUserInfo(oauthToken);

                return new AuthorizationTokenDTO
                {
                    Token = oauthToken,
                    ExternalAccountId = userInfo.UserId,
                    ExternalAccountName = userInfo.UserName,
                    ExternalStateToken = state,
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

        private void ParseCodeAndState(string queryString, out string code, out string state)
        {
            if (string.IsNullOrEmpty(queryString))
            {
                throw new ApplicationException("QueryString is empty.");
            }

            code = null;
            state = null;

            var tokens = queryString.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                var nameValueTokens = token.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (nameValueTokens.Length < 2)
                {
                    continue;
                }

                if (nameValueTokens[0] == "code")
                {
                    code = nameValueTokens[1];
                }
                else if (nameValueTokens[0] == "state")
                {
                    state = nameValueTokens[1];
                }
            }
        }
    }
}