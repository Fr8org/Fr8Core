
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
                    @"00D610000007nIo!AQ8AQHb7zGj8FFNh8Cimj9f_f174biQ3ZYT3TBjFUx_fCrOHZZgBwUusnbKeqOBf5QQdX6w1KpRfoo_LE5KGf78zPbPyL35m",
                AdditionalAttributes =
                    @"refresh_token=5Aep861tbt360sO1.uiSjP9QVIPyR8s6bD9ipi.zZtsHJjep8f9D6MxcRJRKyYoiUo.U.XfZX0wx8JWmboZNVqm;instance_url=https://na34.salesforce.com;api_version=v34.0"
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

        public static ActivityTemplateDTO Create_Contact_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Create_Contact_TEST",
                Label = "Create Contact",
                NeedsAuthentication = true
            };
        }

        public static ActivityTemplateDTO Create_Lead_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Create_Lead_TEST",
                Label = "Create Lead",
                NeedsAuthentication = true
            };
        }

        public static ActivityTemplateDTO Get_Data_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Get_Data_TEST",
                Label = "Get Data from Salesforce.com",
                NeedsAuthentication = true
            };
        }

        public static Fr8DataDTO Create_Account_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Create_Account_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Create Account",
                AuthToken = Salesforce_AuthToken(),
                ActivityTemplate = activityTemplate
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }

        public static Fr8DataDTO Create_Contact_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Create_Contact_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Create Contact",
                AuthToken = Salesforce_AuthToken(),
                ActivityTemplate = activityTemplate
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }

        public static Fr8DataDTO Create_Lead_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Create_Lead_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Create Lead",
                AuthToken = Salesforce_AuthToken(),
                ActivityTemplate = activityTemplate
            };
            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }

        public static ActivityDTO Get_Data_v1_InitialConfiguration_ActionDTO()
        {
            var activityTemplate = Get_Data_v1_ActivityTemplate();

            return new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Get Data from Salesforce.com",
                AuthToken = Salesforce_AuthToken(),
                ActivityTemplate = activityTemplate
            };
        }
    }
}
