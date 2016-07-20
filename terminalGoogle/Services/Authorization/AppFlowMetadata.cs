using System.Collections.Generic;
using System.Web.Mvc;
using Fr8.Infrastructure.Utilities.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Auth.OAuth2.Responses;
using Newtonsoft.Json;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Infrastructure;

namespace terminalGoogle.Services.Authorization
{
    class AppFlowMetadata : FlowMetadata
    {
        private readonly string _googleToken;
        GoogleAuthDTO _googleAuth;
        private readonly string _email;
        private readonly string _authCallbackUrl;
        private IAuthorizationCodeFlow _flow;

        public AppFlowMetadata(string googleToken, string email = null, string callbackUrl = null)
        {
            _googleToken = googleToken;
            _email = email;

            _authCallbackUrl = callbackUrl;
        }

        public AppFlowMetadata(GoogleAuthDTO googleToken, string email = null, string callbackUrl = null)
        {
            _googleAuth = googleToken;
            _email = email;

            _authCallbackUrl = callbackUrl;
        }


        private void SetUserGoogleAuthData(string authData)
        {

        }

        private string GetUserGoogleAuthData()
        {
            TokenResponse tokenResponse = new TokenResponse();
            tokenResponse.AccessToken = _googleAuth.AccessToken;
            tokenResponse.RefreshToken = _googleAuth.RefreshToken;
            tokenResponse.Scope = CloudConfigurationManager.GetSetting("GoogleScope");


            Dictionary<string, string> authDictionary = new Dictionary<string, string>();
            authDictionary.Add(_googleAuth.AccessToken, JsonConvert.SerializeObject(tokenResponse));

            return JsonConvert.SerializeObject(authDictionary);
        }

        public override string GetUserId(Controller controller)
        {
            return _googleToken;
        }

        private IAuthorizationCodeFlow CreateFlow()
        {
                return new AuthorizationCodeFlow(
                    new GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = new ClientSecrets
                        {
                            ClientId = CloudConfigurationManager.GetSetting("GoogleClientId"),
                            ClientSecret = CloudConfigurationManager.GetSetting("GoogleClientSecret")
                        },
                        Scopes = CloudConfigurationManager.GetSetting("GoogleScope").Split(' '),
                        DataStore = new JSONDataStore(
                            GetUserGoogleAuthData, SetUserGoogleAuthData),
                    }, _email);
        }

        public override IAuthorizationCodeFlow Flow
        {
            get { return _flow ?? (_flow = CreateFlow()); }
        }

        public override string AuthCallback
        {
            get { return _authCallbackUrl; }
        }
    }
}