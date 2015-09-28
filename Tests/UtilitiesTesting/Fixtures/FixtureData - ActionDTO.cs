﻿using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static ActionDTO TestActionDTO1()
        {
            return new ActionDTO()
            {
                Name = "test action type",
                ActivityTemplate = FixtureData.TestActivityTemplateDTO1()
            };
        }

        public static ActionDTO TestActionDTOForSalesforce()
        {
            return new ActionDTO()
            {
                Name = "test salesforce action",
                ActivityTemplate = FixtureData.TestActivityTemplateSalesforce()
            };
        }
    }
}