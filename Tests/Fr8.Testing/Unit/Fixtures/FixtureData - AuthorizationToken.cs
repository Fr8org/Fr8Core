using System;
using System.Collections.Generic;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Data.States;
using Newtonsoft.Json;

namespace Fr8.Testing.Unit.Fixtures
{
    partial class FixtureData
    {
        public static void AddTestActivityTemplate()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.TerminalRepository.Add(new TerminalDO
                {
                    Id = FixtureData.GetTestGuidById(1),
                    Name = "testTerminal",
                    Label = "test",
                    Version = "v1",
                    Secret = "test",
                    TerminalStatus = 1,
                    ParticipationState = ParticipationState.Approved,
                    OperationalState = OperationalState.Active,
                    Endpoint="http://localhost:11111"
                });
                uow.SaveChanges();
                uow.ActivityTemplateRepository.Add(GetTestActivityTemplateDO());
                uow.SaveChanges();
            }
        }

        public static ActivityTemplateDO GetTestActivityTemplateDO()
        {
            return new ActivityTemplateDO("Test", "test", "v1", "test", FixtureData.GetTestGuidById(1))
            {
                Id = FixtureData.GetTestGuidById(1),
                Categories = new List<ActivityCategorySetDO>()
            };
        }

        public static void AddAuthorizationToken(Fr8AccountDO user, string externalAccountId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var tokenDO = new AuthorizationTokenDO()
                {
                    UserID = user.Id,
                    TerminalID = FixtureData.GetTestGuidById(1),
                    AuthorizationTokenState = AuthorizationTokenState.Active,
                    ExpiresAt = null,
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


        public static string GetGoogleAuthorizationTokenForGmailMonitor()
        {
            // login: 	 icantcomeupwithauniquename@gmail.com
            // password: grolier34
            return ("{\"AccessToken\":\"ya29.Ci8KAzlXWBf72zj3EbwTXszvXUwV3HZEOGfRoXtBTzhflTatfSqCrT1Acs6MOyJfhA\",\"RefreshToken\":\"1/FEwaD090qJMUkzZHTHnxnSZGHYbqA6v00N5QRq8eWTs\",\"Expires\":\"2016-06-23T20:25:04.4440362+03:00\"}");
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