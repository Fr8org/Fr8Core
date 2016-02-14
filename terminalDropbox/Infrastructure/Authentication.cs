using Data.Interfaces.DataTransferObjects;
using Dropbox.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Utilities.Configuration.Azure;

namespace terminalDropbox.Infrastructure
{
    public class Authentication
    {
        string dropboxAuthCallbackURLDomain = string.Empty;
        string dropboxAuthAppKey = string.Empty;
        string dropboxAuthAppSecret = string.Empty;

        public Authentication()
        {
            dropboxAuthCallbackURLDomain = CloudConfigurationManager.GetSetting("DropboxAuthCallbackURLDomain");
            dropboxAuthAppKey = CloudConfigurationManager.GetSetting("DropboxAuthAppKey");
            dropboxAuthAppSecret = CloudConfigurationManager.GetSetting("DropboxAuthAppSecret");
        }

        private string RedirectURI
        {
            get
            {
                return dropboxAuthCallbackURLDomain + "AuthenticationCallback/ProcessSuccessfulOAuthResponse?terminalName=terminalDropbox&terminalVersion=1";
            }
        }

        public ExternalAuthUrlDTO GetExternalAuthUrl()
        {
            var externalStateToken = Guid.NewGuid().ToString();
            var url = CreateAuthUrl(externalStateToken);
            var externalAuthUrlDTO = new ExternalAuthUrlDTO()
            {
                ExternalStateToken = externalStateToken,
                Url = url
            };
            return externalAuthUrlDTO;
        }

        public async Task<AuthorizationTokenDTO> Authenticate(ExternalAuthenticationDTO externalAuthDTO)
        {
            string code;
            string state;
            ParseCodeAndState(externalAuthDTO.RequestQueryString, out code, out state);
            OAuth2Response response = (OAuth2Response)await GetAuthToken(code);

            if (response == null)
                throw new ArgumentNullException("Unable to get authentication token in dropbox.");

            string externalId = "";
            using (var dbx = new DropboxClient(response.AccessToken))
            {
                externalId = (await dbx.Users.GetCurrentAccountAsync()).AccountId;
            }

            return new AuthorizationTokenDTO()
            {
                Token = response.AccessToken,
                ExternalStateToken = state,
                ExternalAccountId = externalId
            };
        }

        private async Task<OAuth2Response> GetAuthToken(string code)
        {

            OAuth2Response response = null;

            try
            {
                response = await DropboxOAuth2Helper.ProcessCodeFlowAsync(
                          code,
                          dropboxAuthAppKey,
                          dropboxAuthAppSecret,
                          RedirectURI);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }

            return response;
        }

        private string CreateAuthUrl(string externalStateValue)
        {
            var url = DropboxOAuth2Helper.GetAuthorizeUri(
                OAuthResponseType.Code,
                dropboxAuthAppKey,
                RedirectURI,
                externalStateValue);

            return url.ToString();
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