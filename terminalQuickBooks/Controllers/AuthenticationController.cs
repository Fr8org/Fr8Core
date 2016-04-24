using System;
using System.Threading.Tasks;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using TerminalBase.BaseClasses;
using terminalQuickBooks.Interfaces;
using terminalQuickBooks.Services;

namespace terminalQuickBooks.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : BaseTerminalController
    {
        private const string curTerminal = "terminalQuickBooks";

        private readonly IConnectivity _connectivity;


        public AuthenticationController()
        {
            _connectivity = new Connectivity();
        }

        [HttpPost]
        [Route("initial_url")]
        public ExternalAuthUrlDTO GenerateOAuthInitiationURL()
        {
            var url = _connectivity.CreateAuthUrl();

            var externalAuthUrlDTO = new ExternalAuthUrlDTO()
            {
                ExternalStateToken = null,
                Url = url
            };

            return externalAuthUrlDTO;
        }

        [HttpPost]
        [Route("token")]
        public async Task<AuthorizationTokenDTO> GenerateOAuthToken(
            ExternalAuthenticationDTO externalAuthDTO)
        {
            try
            {
                string oauth_token, oauth_verifier, realm_id, data_source;

                ParseCodeAndState(externalAuthDTO.RequestQueryString, out oauth_token, out oauth_verifier, out realm_id, out data_source);

                if (string.IsNullOrEmpty(oauth_token) || string.IsNullOrEmpty(oauth_verifier) || string.IsNullOrEmpty(realm_id) || string.IsNullOrEmpty(data_source))
                {
                    throw new ApplicationException("OAuth Token or OAuth Verifier or Realm ID or Data Source is empty.");
                }

                var oauthToken = await _connectivity.GetOAuthToken(oauth_token, oauth_verifier, realm_id);

                return new AuthorizationTokenDTO()
                {
                    Token = oauthToken,
                    ExternalAccountId = realm_id,
                    ExternalStateToken = null
                };
            }
            catch (Exception ex)
            {
                ReportTerminalError(curTerminal, ex,externalAuthDTO.Fr8UserId);

                return new AuthorizationTokenDTO()
                {
                    Error = "An error occurred while trying to authorize, please try again later."
                };
            }
        }

        private void ParseCodeAndState(string queryString, out string oauth_token, out string oauth_verifier, out string realm_id, out string data_source)
        {
            if (string.IsNullOrEmpty(queryString))
            {
                throw new ApplicationException("QueryString is empty.");
            }

            oauth_token = null;
            oauth_verifier = null;
            realm_id = null;
            data_source = null;
            var tokens = queryString.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                var nameValueTokens = token.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (nameValueTokens.Length < 2)
                {
                    continue;
                }

                if (nameValueTokens[0] == "oauth_token")
                {
                    oauth_token = nameValueTokens[1];
                }
                else if (nameValueTokens[0] == "oauth_verifier")
                {
                    oauth_verifier = nameValueTokens[1];
                }
                else if (nameValueTokens[0] == "realmId")
                {
                    realm_id = nameValueTokens[1];
                }
                else if (nameValueTokens[0] == "dataSource")
                {
                    data_source = nameValueTokens[1];
                }
            }
        }
    }
}