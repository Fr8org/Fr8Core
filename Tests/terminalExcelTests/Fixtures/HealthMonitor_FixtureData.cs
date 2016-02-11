using System;
using Data.Interfaces.DataTransferObjects;

namespace terminalExcelTests.Fixtures
{
    public class HealthMonitor_FixtureData
    {
        public static AuthorizationTokenDTO DocuSign_AuthToken()
        {
            return new AuthorizationTokenDTO()
            {
                Token = @"{ ""Email"": ""freight.testing@gmail.com"", ""ApiPassword"": ""SnByDvZJ/fp9Oesd/a9Z84VucjU="" }"
            };
        }

        public static ActivityTemplateDTO Load_Table_Data_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = 1,
                Name = "Load_Table_Data_TEST",
                Version = "1"
            };
        }

        public static Fr8DataDTO Load_Table_Data_v1_InitialConfiguration_Fr8DataDTO(Guid guid)
        {
            var activityTemplate = Load_Table_Data_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = guid,
                Name = "Load_Table_Data",
                Label = "Load Table Data",
                AuthToken = DocuSign_AuthToken(),
                ActivityTemplate = activityTemplate
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }
    }
}
