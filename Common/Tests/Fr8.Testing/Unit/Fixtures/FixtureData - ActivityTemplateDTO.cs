﻿using Data.Entities;
using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Testing.Unit.Fixtures
{
    partial class FixtureData
    {
        public static ActivityTemplateSummaryDTO TestActivityTemplateDTO1()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Write_To_Sql_Server",
                Version = "1"
            };
        }

        public static ActivityTemplateSummaryDTO TestActivityTemplateSalesforce()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Create_Lead",
                Version = "1"
            };
        }

        public static ActivityTemplateSummaryDTO TestActivityTemplateSendGrid()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Send_Email_Via_SendGrid",
                Version = "1"
            };
        }

        public static ActivityTemplateDO TwilioActivityTemplateDTO()
        {
            return new ActivityTemplateDO
            {
                Id = Guid.NewGuid(),
                Name = "Send_Via_Twilio",
                Version = "1"
            };
        }

        public static ActivityTemplateSummaryDTO ActivityTemplateDTOSelectFr8Object()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Select Fr8 Object",
                Version = "1"
            };
        }

        public static ActivityTemplateSummaryDTO QueryDocuSignActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Query_DocuSign",
                Version = "1",
            };
        }

        public static ActivityTemplateSummaryDTO SaveToGoogleSheetActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Save_To_Google_Sheet",
                Version = "1"
            };
        }

        public static ActivityTemplateSummaryDTO Monitor_Gmail_Inbox_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Monitor_Gmail_Inbox",
                Version = "1"
            };
        }

        public static ActivityTemplateSummaryDTO GetGoogleSheetDataActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Get_Google_Sheet_Data",
                Version = "1"
            };
        }
        public static ActivityTemplateSummaryDTO MonitorFormResponsesActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Monitor_Form_Responses",
                Version = "1"
            };
        }
        public static ActivityTemplateSummaryDTO BuildMessageActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Build_Message",
                Version = "1"
            };
        }
        public static ActivityTemplateSummaryDTO SaveToFr8WarehouseActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Save_To_Fr8_Warehouse",
                Version = "1"
            };
        }

        public static ActivityTemplateSummaryDTO GetFileListActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO
            {
                Name = "Get_File_List",
                Version = "1"
            };
        }
    }
}