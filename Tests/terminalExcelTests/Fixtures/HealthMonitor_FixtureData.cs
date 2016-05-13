using System;
using Fr8Data.DataTransferObjects;

namespace terminalExcelTests.Fixtures
{
    public class HealthMonitor_FixtureData
    {
        //public static AuthorizationTokenDTO DocuSign_AuthToken()
        //{
        //    return new AuthorizationTokenDTO()
        //    {
        //        Token = @"{ ""Email"": ""freight.testing@gmail.com"", ""ApiPassword"": ""SnByDvZJ/fp9Oesd/a9Z84VucjU="" }"
        //    };
        //}

        public static string GetFilePath()
        {
            return "https://yardstore1.blob.core.windows.net/default-container-dev/EmailList.xlsx";
        }

        public static ActivityTemplateDTO Load_Table_Data_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Load_Excel_File_TEST",
                Version = "1"
            };
        }

        public static Fr8DataDTO Load_Table_Data_v1_InitialConfiguration_Fr8DataDTO(Guid guid)
        {
            var activityTemplate = Load_Table_Data_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = guid,
                Label = "Load Table Data",
                ActivityTemplate = activityTemplate,
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }
    }
}
