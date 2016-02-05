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
                    @"{ ""Email"": ""freight.testing@gmail.com"", ""ApiPassword"": ""SnByDvZJ/fp9Oesd/a9Z84VucjU="" }",
                AdditionalAttributes =
                    @"refresh_token=5Aep861tbt360sO1.uiSjP9QVIPyR8s6bD9ipi.zZtsHJjep8c8djasP2.kmjMlBpojryMUY4dMgo3rESsiruiR;instance_url=https://na34.salesforce.com;api_version=v34.0"
            };
        }

        public static ActivityTemplateDO GetDataActivityTemplateDO()
        {
            return new ActivityTemplateDO
            {
                Version = "1",
                Name = "Get_Data",
                Label = "Get Data",
                NeedsAuthentication = true
            };
        }

        public static ActivityDO GetFileListTestActionDO1()
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
