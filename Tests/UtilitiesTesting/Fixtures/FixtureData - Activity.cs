using Core.Interfaces;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Data.Wrappers;
using Newtonsoft.Json;
using StructureMap;
using System.Collections.Generic;
using System;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {




        public static ActionDO TestActionWriteSqlServer1()
        {
            var actionTemplate = ActionTemplate();

            var curActionDO = new ActionDO
            {
                Id = 54,
                Name = "Write to Sql Server",
                CrateStorage = "",
                Ordering = 1,
                ActionState = ActionState.Unstarted,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
            return curActionDO;
        }



        public static ActionDO TestAction1()
        {
            var actionTemplate = ActionTemplate();
            var curActionDO = new ActionDO
            {
                Name = "Action 1",
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate,
            };
            return curActionDO;
        }

        public static ActionDO TestAction2()
        {
            var actionTemplate = ActionTemplate();
            var curActionDO = new ActionDO
            {
                Id = 2,
                Name = "Action 2",
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
            return curActionDO;
        }

        
        

        public static AuthorizationTokenDO TestActionAuthenticate3()
        {
            AuthorizationTokenDO curAuthorizationTokenDO = new AuthorizationTokenDO()
            {
                Token = "TestToken",
                AuthorizationTokenState = AuthorizationTokenState.Revoked
            };
            return curAuthorizationTokenDO;
        }

    }
}