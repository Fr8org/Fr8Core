using System;
using System.Collections.Generic;
using Data.Control;
using Data.Crates;
using Newtonsoft.Json;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Interfaces;
using Hub.Managers;
using Newtonsoft.Json.Linq;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static void AddTestActivityTemplate()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.TerminalRepository.Add(new TerminalDO
                {
                    Id = 1,
                    Name = "testTerminal",
                    Label = "test",
                    Version = "v1",
                    PublicIdentifier = "test",
                    Secret = "test",
                    TerminalStatus = 1
                });
                uow.SaveChanges();
                uow.ActivityTemplateRepository.Add(GetTestActivityTemplateDO());
                uow.SaveChanges();
            }
        }

        public static ActivityTemplateDO GetTestActivityTemplateDO()
        {
            return new ActivityTemplateDO("Test", "test", "v1", "test", 1)
            {
                Id = FixtureData.GetTestGuidById(1)
            };
        }

        public static void AddAuthorizationToken(Fr8AccountDO user, string externalAccountId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var tokenDO = new AuthorizationTokenDO()
                {
                    UserID = user.Id,
                    TerminalID = 1,
                    AuthorizationTokenState = AuthorizationTokenState.Active,
                    ExpiresAt = DateTime.UtcNow.AddYears(100),
                    Token = @"{""Email"":""64684b41-bdfd-4121-8f81-c825a6a03582"",""ApiPassword"":""HyCXOBeGl/Ted9zcMqd7YEKoN0Q=""}",
                    ExternalAccountId = externalAccountId
                };
                uow.AuthorizationTokenRepository.Add(tokenDO);
                uow.SaveChanges();
            }
        }

        public static AuthorizationTokenDTO AuthorizationTokenTest1()
        {
            return new AuthorizationTokenDTO()
            {
                Token = "bLgeJYcIkHAAAAAAAAAAFf6hjXX_RfwsFNTfu3z00zrH463seBYMNqBaFpbfBmqf",
                ExternalAccountId = "ExternalAccountId"
            };
        }
        
        public static AuthorizationTokenDTO GetGoogleAuthorizationToken()
        {
            return new AuthorizationTokenDTO
            {
                Token = @"{""AccessToken"":""ya29.CjHXAnhqySXYWbq-JE3Nqpq18L_LGYw3xx_T-lD6jeQd6C2mMoKzQhTWRWFSkPcX-pH_"",""RefreshToken"":""1/ZmUihiXxjwiVd-kQe46hDXKB95VaHM5yP-6bfrS-EUUMEudVrK5jSpoR30zcRFq6"",""Expires"":""2017-11-28T13:29:12.653075+05:00""}"
            };
        }

        /// <summary>
        /// Dropbox auth token
        /// </summary>
        /// <returns> </returns>
        public static AuthorizationTokenDTO GetDropboxAuthorizationToken()
        {
            return new AuthorizationTokenDTO
            {
                Token = @"odQGb-zMEiAAAAAAAAAANnouWOHmMTge7bJxucikAlb8sEXaMDAXXFdowLFBSyTn"
            };
        }
    }
}