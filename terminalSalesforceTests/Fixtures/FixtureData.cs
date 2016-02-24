using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;

namespace terminalSalesforceTests.Fixtures
{
    public class FixtureData
    {
        public static AuthorizationTokenDO Salesforce_AuthToken()
        {
            return new AuthorizationTokenDO()
            {
                Token =
                    @"00D610000007nIo!AQ8AQHb7zGj8FFNh8Cimj9f_f174biQ3ZYT3TBjFUx_fCrOHZZgBwUusnbKeqOBf5QQdX6w1KpRfoo_LE5KGf78zPbPyL35m",
                AdditionalAttributes =
                    @"refresh_token=5Aep861tbt360sO1.uiSjP9QVIPyR8s6bD9ipi.zZtsHJjep8f9D6MxcRJRKyYoiUo.U.XfZX0wx8JWmboZNVqm;instance_url=https://na34.salesforce.com;api_version=v34.0"
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
