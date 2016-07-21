using Dropbox.Api;
using System;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities.Configuration;

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
            string code = GetTokenValueByKey(externalAuthDTO.RequestQueryString, "code");
            string state = GetTokenValueByKey(externalAuthDTO.RequestQueryString, "state");

            OAuth2Response response = await GetAuthToken(code);

            if (response == null)
                throw new ArgumentNullException("Unable to get authentication token in dropbox.");

            string externalId = "";
            using (var dbx = new DropboxClient(response.AccessToken))
            {
                externalId = (await dbx.Users.GetCurrentAccountAsync()).Email;
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

        private string GetTokenValueByKey(string queryString, string tokenKey)
        {
            if (String.IsNullOrEmpty(queryString))
                throw new ApplicationException("QueryString is empty.");
            if (String.IsNullOrWhiteSpace(tokenKey))
                throw new ArgumentNullException(nameof(tokenKey));

            var tokenDictionary = queryString.Split('&')
                .Select(token => token.Split('='))
                .ToDictionary(x => x[0], x => x[1]);
            return tokenDictionary[tokenKey];
        }
    }
}