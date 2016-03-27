using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Data.Constants;
using Data.Crates;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.DataTransferObjects.Helpers;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using StructureMap;

namespace terminalIntegrationTests.Fixtures
{
    public class HealthMonitor_FixtureData
    {
        public static PlanEmptyDTO TestPlanEmptyDTO()
        {
            return new PlanEmptyDTO()
            {
                Name = "Integratin Test Plan",
                Description = "Create a new Integration test Plan and configure with custom activities"
            };
        }

        public static PlanDO TestPlan_CanCreate(string name)
        {
            var curPlanDO = new PlanDO
            {
                Id = Guid.NewGuid(),
                Description = name,
                Name = name,
                PlanState = PlanState.Active,
            };
            return curPlanDO;
        }

        public static SubPlanDO TestSubPlanHealthDemo(Guid parentNodeId)
        {
            var SubPlanDO = new SubPlanDO
            {
                Id = Guid.NewGuid(),
                ParentPlanNodeId = parentNodeId,
                RootPlanNodeId = parentNodeId,
                NodeTransitions = "[{'TransitionKey':'true','ProcessNodeId':'2'}]"
            };
            return SubPlanDO;
        }


        public static AuthorizationTokenDTO Google_AuthToken()
        {
            return new AuthorizationTokenDTO
            {
                Token =
                    @"{""AccessToken"":""ya29.qgKKAej9ABzUTVL9y04nEtlo0_Qlpk_dqIBLmg1k7tBo__Dgab0TWvSf-ZgjrjRmUA"",""RefreshToken"":""1/x3T7UajSlqgYQa2BeBozc_49Sa29zCqe-EEvi5eBfFF90RDknAdJa_sgfheVM0XT"",""Expires"":""2017-03-19T13:24:33.2805735+01:00""}"
            };
        }
    }
}
