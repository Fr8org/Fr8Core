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

        public static Fr8DataDTO Query_DocuSign_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Query_DocuSign_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Query DocuSign",
                ActivityTemplate = activityTemplate
            };

            return ConvertToFr8Data(activityDTO);
        }
        

        public static Fr8DataDTO Save_To_Google_Sheet_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Save_To_Google_Sheet_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Save To Google Sheet",
                AuthToken = Google_AuthToken(),
                ActivityTemplate = activityTemplate
            };

            return ConvertToFr8Data(activityDTO);
        }

        public static ActivityTemplateDTO Query_DocuSign_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = 1,
                Name = "Query_DocuSign",
                Version = "1",
                Terminal = new TerminalDTO()
                {
                    
                }
            };
        }

        public static ActivityTemplateDTO Save_To_Google_Sheet_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = 1,
                Name = "Save_To_Google_Sheet",
                Version = "1"
            };
        }

        private static Fr8DataDTO ConvertToFr8Data(ActivityDTO activityDTO)
        {
            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }

        public static string DocuSignTerminalUrl()
        {
            return ConfigurationManager.AppSettings["terminalDocuSignUrl"];
        }

        public static string GoogleTerminalUrl()
        {
            return ConfigurationManager.AppSettings["terminalGoogleUrl"];
        }

        public static AuthorizationTokenDTO Google_AuthToken()
        {
            //return new AuthorizationTokenDTO
            //{
            //    Token =
            //        @"{""AccessToken"":""ya29.qgKKAej9ABzUTVL9y04nEtlo0_Qlpk_dqIBLmg1k7tBo__Dgab0TWvSf-ZgjrjRmUA"",""RefreshToken"":""1/x3T7UajSlqgYQa2BeBozc_49Sa29zCqe-EEvi5eBfFF90RDknAdJa_sgfheVM0XT"",""Expires"":""2016-03-19T13:24:33.2805735+01:00""}"
            //};

            return new AuthorizationTokenDTO()
            {
                Token = @"{""AccessToken"":""ya29.OgLf-SvZTHJcdN9tIeNEjsuhIPR4b7KBoxNOuELd0T4qFYEa001kslf31Lme9OQCl6S5"",""RefreshToken"":""1/04H9hNCEo4vfX0nHHEdViZKz1CtesK8ByZ_TOikwVDc"",""Expires"":""2015-11-28T13:29:12.653075+05:00""}"
            };
        }
    }
}
