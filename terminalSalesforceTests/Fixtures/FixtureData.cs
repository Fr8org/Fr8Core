using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Salesforce.Common;

namespace terminalSalesforceTests.Fixtures
{
    public class FixtureData
    {
        public static async Task<AuthorizationTokenDO>  Salesforce_AuthToken()
        {
            var auth = new AuthenticationClient();
            await auth.UsernamePasswordAsync(
                "3MVG9KI2HHAq33RzZO3sQ8KU8JPwmpiZBpe_fka3XktlR5qbCWstH3vbAG.kLmaldx8L1V9OhqoAYUedWAO_e",
                "611998545425677937",
                "alex@dockyard.company",
                "thales@34");

            return new AuthorizationTokenDO()
            {
                Token = auth.AccessToken,
                AdditionalAttributes = string.Format("refresh_token=;instance_url={0};api_version={1}", auth.InstanceUrl, auth.ApiVersion)
            };
        }

        public static ActivityTemplateDO GetDataActivityTemplateDO()
        {
            return new ActivityTemplateDO
            {
                Version = "1",
                Name = "Get_Data",
                Label = "Get Data from Salesforce.com",
                NeedsAuthentication = true
            };
        }

        public static ActivityDO GetFileListTestActivityDO1()
        {
            var actionTemplate = GetDataActivityTemplateDO();

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
