using System.Collections.Generic;
using System.Web.Mvc;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;
using Newtonsoft.Json;
using Utilities.Configuration.Azure;
using terminalGoogle.DataTransferObjects;
using Google.Apis.Auth.OAuth2.Responses;

namespace terminalGoogle.Services
{
    class AppFlowMetadata : FlowMetadata
    {
        private readonly string _googleToken;
        GoogleAuthDTO _googleAuth;
        private readonly string _email;
        private readonly string _authCallbackUrl;
        private IAuthorizationCodeFlow _flow;
        private AuthData _authDataService;

        public AppFlowMetadata(string googleToken, string email = null, string callbackUrl = null)
        {
            _googleToken = googleToken;
            _email = email;

            _authCallbackUrl = callbackUrl;
            _authDataService = new AuthData();
        }

        public AppFlowMetadata(GoogleAuthDTO googleToken, string email = null, string callbackUrl = null)
        {
            _googleAuth = googleToken;
            _email = email;

            _authCallbackUrl = callbackUrl;
            _authDataService = new AuthData();
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