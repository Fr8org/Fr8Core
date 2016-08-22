using Fr8.Infrastructure.Utilities.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Interfaces;
using terminalGoogle.Services.Authorization;

namespace terminalGoogle.Services
{
    public class GoogleGmail : IGoogleGmail
    {
        private GoogleAuthorizer _googleAuth;

        public GoogleGmail()
        {
            _googleAuth = new GoogleAuthorizer();
        }

        public async Task<GmailService> CreateGmailService(GoogleAuthDTO authDTO)
        {
            var flowData = _googleAuth.CreateFlowMetadata(authDTO, "", CloudConfigurationManager.GetSetting("GoogleRedirectUri"));
            TokenResponse tokenResponse = new TokenResponse();
            tokenResponse.AccessToken = authDTO.AccessToken;
            tokenResponse.RefreshToken = authDTO.RefreshToken;
            tokenResponse.Scope = CloudConfigurationManager.GetSetting("GoogleScope");

            UserCredential userCredential;
            try
            {
                userCredential = new UserCredential(flowData.Flow, authDTO.AccessToken, tokenResponse);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredential,
                ApplicationName = "Fr8",
            });

            return service;
        }
    }
}