using System;
using System.Threading.Tasks;
using Data.Entities;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using Salesforce.Common;
using StructureMap;
using Newtonsoft.Json;

namespace terminalSalesforceTests.Fixtures
{
    public class FixtureData
    {
        public static async Task<AuthorizationToken>  Salesforce_AuthToken()
        {
            var auth = new AuthenticationClient();

            return new AuthorizationToken()
            {
                Token = JsonConvert.SerializeObject(new { AccessToken = auth.AccessToken }),
                AdditionalAttributes = $"instance_url={auth.InstanceUrl};api_version={auth.ApiVersion}"
            };
        }

        public static ActivityTemplateSummaryDTO GetDataActivityTemplateDTO()
        {
            return new ActivityTemplateSummaryDTO
            {
                Version = "1",
                Name = "Get_Data"
            };
        }

        public static ActivityTemplateSummaryDTO SaveToSalesforceActivityTemplateDTO()
        {
            return new ActivityTemplateSummaryDTO
            {
                Version = "1",
                Name = "Save_To_SalesforceDotCom"
            };
        }

        public static ActivityTemplateDO PostToChatterActivityTemplateDO()
        {
            return new ActivityTemplateDO
            {
                Version = "1",
                Name = "Post_To_Chatter",
                Label = "Post To Chatter",
                NeedsAuthentication = true
            };
        }

        public static async Task<ActivityContext> GetFileListTestActivityContext1()
        {
            var actionTemplate = GetDataActivityTemplateDTO();

            var activityPayload = new ActivityPayload()
            {
                Id = new Guid("8339DC87-F011-4FB1-B47C-FEC406E4100A"),
                ActivityTemplate = actionTemplate,
                CrateStorage = new CrateStorage()
            };
            var activityContext = new ActivityContext()
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = activityPayload,
                AuthorizationToken = await Salesforce_AuthToken()
            };
            return activityContext;
        }
        public static async Task<ActivityContext> GetFileListTestActivityContext2()
        {
            var actionTemplate = GetDataActivityTemplateDTO();

            var activityPayload = new ActivityPayload()
            {
                Id = new Guid("8339DC87-F011-4FB1-B47C-FEC406E4100A"),
                ActivityTemplate = actionTemplate,
                CrateStorage = new CrateStorage()
            };
            var activityContext = new ActivityContext()
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = activityPayload,
                AuthorizationToken = await FixtureData.Salesforce_AuthToken()
            };
            return activityContext;
        }

        public static ActivityContext SaveToSalesforceTestActivityContext1()
        {
            var actionTemplate = SaveToSalesforceActivityTemplateDTO();

            var activityPayload = new ActivityPayload()
            {
                Id = new Guid("8339DC87-F011-4FB1-B47C-FEC406E4100A"),
                ActivityTemplate = actionTemplate,
                CrateStorage = new CrateStorage(),
            };
            var activityContext = new ActivityContext()
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = activityPayload
            };

            return activityContext;
        }

        public static async Task<ActivityContext> SaveToSalesforceTestActivityContext()
        {
            var activityTemplate = SaveToSalesforceActivityTemplateDTO();
            return new ActivityContext
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                AuthorizationToken = await Salesforce_AuthToken(),
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage(),
                    Id = new Guid("8339DC87-F011-4FB1-B47C-FEC406E4100A"),
                    ActivityTemplate = activityTemplate
                },
            };
        }

        public static ActivityDO PostToChatterTestActivityDO1()
        {
            var actionTemplate = PostToChatterActivityTemplateDO();

            var activityDO = new ActivityDO()
            {
                Id = new Guid("8339DC87-F011-4FB1-B47C-FEC406E4100A"),
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate,
                CrateStorage = "",

            };
            return activityDO;
        }
    }
}
