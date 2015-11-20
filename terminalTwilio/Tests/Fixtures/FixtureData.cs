using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Crates;
using Data.Interfaces.Manifests;
using Newtonsoft.Json;

namespace terminalTwilio.Tests.Fixtures
{
    public class FixtureData
    {
        public static Guid TestGuid_Id_57()
        {
            return new Guid("A1C11E86-9B54-42D4-AA91-605BF46E68E9");
        }

        public static ActionDO ConfigureTwilioAction()
        {
            var actionTemplate = FixtureData.TwilioActionTemplateDTO();

            var actionDO = new ActionDO()
            {
                Name = "testaction",
                Id = TestGuid_Id_57(),
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate,
                CrateStorage = "",
            };

            return actionDO;
        }

        public static ActivityTemplateDO TwilioActionTemplateDTO()
        {
            return new ActivityTemplateDO
            {
                Id = 1,
                Name = "Send_Via_Twilio",
                Version = "1"
            };
        }

        public static Crate CrateDTOForTwilioConfiguration()
        {
            var confControls = JsonConvert.DeserializeObject<StandardConfigurationControlsCM>("{\"Controls\":[{\"groupName\":\"SMSNumber_Group\",\"radios\":[{\"selected\":false,\"name\":\"SMSNumberOption\",\"value\":null,\"controls\":[{\"name\":\"SMS_Number\",\"required\":true,\"value\":null,\"label\":\"SMS Number\",\"type\":\"TextBox\",\"selected\":false,\"events\":null,\"source\":null}]},{\"selected\":true,\"name\":\"SMSNumberOption\",\"value\":null,\"controls\":[{\"listItems\":[{\"Key\":\"+15005550006\",\"Value\":\"+15005550006\"}],\"name\":\"upstream_crate\",\"required\":false,\"value\":\"+15005550006\",\"label\":\"a value from Upstream Crate:\",\"type\":\"DropDownList\",\"selected\":false,\"events\":[{\"name\":\"onChange\",\"handler\":\"requestConfig\"}],\"source\":{\"manifestType\":\"Standard Design-Time Fields\",\"label\":\"Available Fields\"}}]}],\"name\":null,\"required\":false,\"value\":null,\"label\":\"For the SMS Number use:\",\"type\":\"RadioButtonGroup\",\"selected\":false,\"events\":null,\"source\":null},{\"name\":\"SMS_Body\",\"required\":true,\"value\":\"DocuSign Sent\",\"label\":\"SMS Body\",\"type\":\"TextBox\",\"selected\":false,\"events\":null,\"source\":null}]}", new ControlDefinitionDTOConverter());

            return Data.Crates.Crate.FromContent("Configuration_Controls", confControls);
        }
        public static AuthorizationTokenDO AuthTokenDOTest1()
        {
            return new AuthorizationTokenDO()
            {
                Token = @"{""Email"":""docusign_developer@dockyard.company"",""ApiPassword"":""VIXdYMrnnyfmtMaktD+qnD4sBdU=""}",
                ExternalAccountId = "docusign_developer@dockyard.company",
                UserID = "0addea2e-9f27-4902-a308-b9f57d811c0a",

            };
        }
    }
}