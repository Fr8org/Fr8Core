using System;
using System.Collections.Generic;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using Newtonsoft.Json;
using StructureMap;
using System.Configuration;

namespace terminalTwilio.Tests.Fixtures
{
    public class FixtureData
    {
        public static Guid TestGuid_Id_57()
        {
            return new Guid("A1C11E86-9B54-42D4-AA91-605BF46E68E9");
        }

        public static ActivityContext ConfigureTwilioActivity()
        {
            var actionTemplate = TwilioActivityTemplateDTO();

            var activityPayload = new ActivityPayload
            {
                Id = TestGuid_Id_57(),
                ActivityTemplate = actionTemplate,
                CrateStorage = new CrateStorage()
            };
            var activityContext = new ActivityContext
            {
                ActivityPayload = activityPayload,
                AuthorizationToken = AuthTokenDOTest1()
            };
            return activityContext;
        }

        public static ActivityTemplateSummaryDTO TwilioActivityTemplateDTO()
        {
            return new ActivityTemplateSummaryDTO
            {
                Name = "Send_Via_Twilio",
                Version = "1"
            };
        }

        public static Crate CrateDTOForTwilioConfiguration()
        {
            var confControls =
                JsonConvert.DeserializeObject<StandardConfigurationControlsCM>(
                    "{\"Controls\": [{\"initialLabel\": \"For the SMS Number Use:\",\"upstreamSourceLabel\": null,\"valueSource\": \"specific\",\"listItems\": [],\"name\": \"Recipient\",\"required\": false,\"TextValue\": \""+ ConfigurationManager.AppSettings["TestPhoneNumber"] + "\",\"label\": null,\"type\": \"TextSource\",\"selected\": false,\"events\": null,\"source\": {\"manifestType\": \"Standard Design-Time Fields\",\"label\": \"Upstream Terminal-Provided Fields\"}},{\"initialLabel\": \"For the SMS Number Use:\",\"upstreamSourceLabel\": null,\"valueSource\": \"specific\",\"listItems\": [],\"name\": \"SMS_Body\",\"required\": true,\"TextValue\": \"Unit Test Message\",\"label\": \"SMS Body\",\"type\": \"TextSource\",\"selected\": false,\"events\": null,\"source\": {\"manifestType\": \"Standard Design-Time Fields\",\"label\": \"Upstream Terminal-Provided Fields\"}}]}",
                    new ControlDefinitionDTOConverter());

            return Crate.FromContent("Configuration_Controls", confControls);
        }

        public static AuthorizationToken AuthTokenDOTest1()
        {
            return new AuthorizationToken
            {
                Token =
                    @"{""Email"":""docusign_developer@dockyard.company"",""ApiPassword"":""VIXdYMrnnyfmtMaktD+qnD4sBdU=""}",
                ExternalAccountId = "docusign_developer@dockyard.company",
                UserId = "0addea2e-9f27-4902-a308-b9f57d811c0a"

            };
        }
    }
}