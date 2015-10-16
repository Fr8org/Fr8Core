﻿using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static ActivityTemplateDTO TestActivityTemplateDTO1()
        {
            return new ActivityTemplateDTO()
            {
                Id = 1,
                Name = "Write_To_Sql_Server",
                Version = "1"
            };
        }

        public static ActivityTemplateDTO TestActivityTemplateSalesforce()
        {
            return new ActivityTemplateDTO()
            {
                Id = 1,
                Name = "Create_Lead",
                Version = "1"
            };
        }

        public static ActivityTemplateDTO TestActivityTemplateSendGrid()
        {
            return new ActivityTemplateDTO()
            {
                Id = 1,
                Name = "SendEmailViaSendGrid",
                Version = "1"
            };
        }

        public static ActivityTemplateDO TwilioActionTemplateDTO()
        {
            return new ActivityTemplateDO
            {
                Id = 1,
                Name = "Send_Via_Twilio",
                Version = "1"
            };
        }
    }
}