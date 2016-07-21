﻿using Data.Entities;
using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Testing.Unit.Fixtures
{
    partial class FixtureData
    {
        public static ActivityTemplateDTO TestActivityTemplateDTO1()
        {
            return new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Write_To_Sql_Server",
                Version = "1"
            };
        }

        public static ActivityTemplateDTO TestActivityTemplateSalesforce()
        {
            return new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Create_Lead",
                Version = "1"
            };
        }

        public static ActivityTemplateDTO TestActivityTemplateSendGrid()
        {
            return new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
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

        public static ActivityTemplateDTO ActivityTemplateDTOSelectFr8Object()
        {
            return new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Select Fr8 Object",
                Version = "1"
            };
        }

        public static ActivityTemplateDTO QueryDocuSignActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Query_DocuSign",
                Version = "1",
            };
        }

        public static ActivityTemplateDTO SaveToGoogleSheetActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Save_To_Google_Sheet",
                Version = "1"
            };
        }

        public static ActivityTemplateDTO Monitor_Gmail_Inbox_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Monitor_Gmail_Inbox",
                Version = "1"
            };
        }

        public static ActivityTemplateDTO GetGoogleSheetDataActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Get_Google_Sheet_Data",
                Version = "1"
            };
        }
        public static ActivityTemplateDTO MonitorFormResponsesActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Monitor_Form_Responses",
                Version = "1"
            };
        }
        public static ActivityTemplateDTO BuildMessageActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Build_Message",
                Version = "1"
            };
        }
        public static ActivityTemplateDTO SaveToFr8WarehouseActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Save_To_Fr8_Warehouse",
                Version = "1"
            };
        }

        public static ActivityTemplateDTO GetFileListActivityTemplate()
        {
            return new ActivityTemplateDTO
            {
                Id = Guid.NewGuid(),
                Name = "Get_File_List",
                Version = "1"
            };
        }
    }
}