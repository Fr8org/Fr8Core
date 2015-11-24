using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StructureMap;
using Data.Interfaces;
using Data.Entities;
using Google.Apis.Auth.OAuth2.Responses;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using terminalGoogle.DataTransferObjects;
using Utilities.Configuration.Azure;

namespace terminalGoogle.Services
{
    public class AuthData
    {
        public void SetUserAuthData(string userId, string providerName, string authData)
        {
           //saving of authdata is in initial_url in terminal controller
        }

        public string GetUserAuthData(string userId, string providerName)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curUserAuthData = uow.AuthorizationTokenRepository.GetAll().Where(i => i.Token.Contains(userId)).DefaultIfEmpty(new AuthorizationTokenDO()).FirstOrDefault();
                GoogleAuthDTO googleOAuth = JsonConvert.DeserializeObject<GoogleAuthDTO>(curUserAuthData.Token);
                TokenResponse tokenResponse = new TokenResponse();
                tokenResponse.AccessToken = googleOAuth.AccessToken;
                tokenResponse.RefreshToken = googleOAuth.RefreshToken;
                tokenResponse.Scope = CloudConfigurationManager.GetSetting("GoogleScope");

                
                Dictionary<string, string> authDictionary = new Dictionary<string, string>();
                authDictionary.Add(userId, JsonConvert.SerializeObject(tokenResponse));

                return JsonConvert.SerializeObject(authDictionary);
            }
        }
    }
}
