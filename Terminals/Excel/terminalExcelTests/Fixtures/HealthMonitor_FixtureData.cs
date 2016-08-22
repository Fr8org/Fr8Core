using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

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

        public static string GetFilePath_OneRowWithWithHeader()
        {
            return "https://yardstore1.blob.core.windows.net/default-container-dev/OneRow_WithHeader.xlsx";
        }

        public static ActivityTemplateSummaryDTO Load_Table_Data_v1_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Load_Excel_File_TEST",
                Version = "1"
            };
        }

        public static Fr8DataDTO Load_Excel_File_v1_InitialConfiguration_Fr8DataDTO(Guid guid)
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
