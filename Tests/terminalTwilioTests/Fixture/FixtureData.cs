using System;
using System.Collections.Generic;
using Data.Entities;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Newtonsoft.Json;

namespace terminalTwilio.Tests.Fixtures
{
    public class FixtureData
    {
        public static Guid TestGuid_Id_57()
        {
            return new Guid("A1C11E86-9B54-42D4-AA91-605BF46E68E9");
        }

        public static ActivityDO ConfigureTwilioActivity()
        {
            var actionTemplate = TwilioActivityTemplateDTO();

            var activityDO = new ActivityDO
            {
                Id = TestGuid_Id_57(),
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate,
                CrateStorage = ""
            };

            return activityDO;
        }

        public static ActivityTemplateDO TwilioActivityTemplateDTO()
        {
            return new ActivityTemplateDO
            {
                Id = Guid.NewGuid(),
                Name = "Send_Via_Twilio",
                Version = "1"
            };
        }

        public static Crate CrateDTOForTwilioConfiguration()
        {
            var confControls =
                JsonConvert.DeserializeObject<StandardConfigurationControlsCM>(
                    "{\"Controls\": [{\"initialLabel\": \"For the SMS Number Use:\",\"upstreamSourceLabel\": null,\"valueSource\": \"specific\",\"listItems\": [],\"name\": \"Recipient\",\"required\": false,\"TextValue\": \"+15005550006\",\"label\": null,\"type\": \"TextSource\",\"selected\": false,\"events\": null,\"source\": {\"manifestType\": \"Standard Design-Time Fields\",\"label\": \"Upstream Terminal-Provided Fields\"}},{\"initialLabel\": \"For the SMS Number Use:\",\"upstreamSourceLabel\": null,\"valueSource\": \"specific\",\"listItems\": [],\"name\": \"SMS_Body\",\"required\": true,\"TextValue\": \"Unit Test Message\",\"label\": \"SMS Body\",\"type\": \"TextSource\",\"selected\": false,\"events\": null,\"source\": {\"manifestType\": \"Standard Design-Time Fields\",\"label\": \"Upstream Terminal-Provided Fields\"}}]}",
                    new ControlDefinitionDTOConverter());

            return Crate.FromContent("Configuration_Controls", confControls);
        }

        public static AuthorizationTokenDO AuthTokenDOTest1()
        {
            return new AuthorizationTokenDO
            {
                Token =
                    @"{""Email"":""docusign_developer@dockyard.company"",""ApiPassword"":""VIXdYMrnnyfmtMaktD+qnD4sBdU=""}",
                ExternalAccountId = "docusign_developer@dockyard.company",
                UserID = "0addea2e-9f27-4902-a308-b9f57d811c0a"

            };
        }

        public static FieldDescriptionsCM TestFields()
        {
            return new FieldDescriptionsCM
            {
                Fields = new List<FieldDTO>
                {
                    new FieldDTO("key", "value")
                }
            };
        }
    }
}