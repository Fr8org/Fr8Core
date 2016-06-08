using System;
using System.Collections.Generic;
using fr8.Infrastructure.Data.Crates;
using fr8.Infrastructure.Data.DataTransferObjects;
using fr8.Infrastructure.Data.Manifests;
using Newtonsoft.Json;
using StructureMap;
using TerminalBase.Infrastructure;
using TerminalBase.Models;

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
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = activityPayload,
                AuthorizationToken = AuthTokenDOTest1()
            };
            return activityContext;
        }

        public static ActivityTemplateDTO TwilioActivityTemplateDTO()
        {
            return new ActivityTemplateDTO
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