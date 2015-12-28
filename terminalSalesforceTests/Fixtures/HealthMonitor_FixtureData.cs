
using Data.Interfaces.DataTransferObjects;
using System;

namespace terminalSalesforceTests.Fixtures
{
    public static class HealthMonitor_FixtureData
    {
        public static AuthorizationTokenDTO Salesforce_AuthToken()
        {
            return new AuthorizationTokenDTO()
            {
                Token =
                    @"{ ""Email"": ""freight.testing@gmail.com"", ""ApiPassword"": ""SnByDvZJ/fp9Oesd/a9Z84VucjU="" }",
                AdditionalAttributes =
                    @"refresh_token=5Aep861tbt360sO1.uiSjP9QVIPyR8s6bD9ipi.zZtsHJjep8eXTEwlpwx8gvOTG_tDqWppOeNVeI33honwW02D;instance_url=https://na34.salesforce.com;api_version=v34.0"
            };
        }

        public static ActivityTemplateDTO Create_Account_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Create_Account_TEST",
                Label = "Create Account",
                NeedsAuthentication = true
            };
        }

        public static ActionDTO Create_Account_v1_InitialConfiguration_ActionDTO()
        {
            var activityTemplate = Create_Account_v1_ActivityTemplate();

            return new ActionDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Create_Account",
                Label = "Create Account",
                AuthToken = Salesforce_AuthToken(),
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id
            };
        }
    }
}
