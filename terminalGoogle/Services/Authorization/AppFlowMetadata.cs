using System.Collections.Generic;
using System.Web.Mvc;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;
using Newtonsoft.Json;
using StructureMap;
using Data.Infrastructure;
using Data.Interfaces;
using Hub.Services;
using Utilities.Configuration.Azure;

namespace terminalGoogle.Services
{
    class AppFlowMetadata : FlowMetadata
    {
        private readonly string _userId;
        private readonly string _email;
        private readonly string _authCallbackUrl;
        private IAuthorizationCodeFlow _flow;
        private AuthData _authDataService;

        public AppFlowMetadata(string userId, string email = null, string callbackUrl = null)
        {
            _userId = userId;
            _email = email;

            _authCallbackUrl = callbackUrl;
            _authDataService = new AuthData();
        }

        private void SetUserGoogleAuthData(string authData)
        {
            _authDataService.SetUserAuthData(_userId, "Google", authData);
        }

        private string GetUserGoogleAuthData()
        {
            return _authDataService.GetUserAuthData(_userId, "Google");
        }

        public override string GetUserId(Controller controller)
        {
            return _userId;
        }

        private IAuthorizationCodeFlow CreateFlow()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
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