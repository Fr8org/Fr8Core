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
    }
}