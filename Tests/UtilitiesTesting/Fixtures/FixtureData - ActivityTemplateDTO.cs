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
        public static ActivityTemplateDTO TestActivityTemplateDTO1()
        {
            return new ActivityTemplateDTO()
            {
                Id = 1,
                Name = "Write_To_Sql_Server",
                Version = "1"
            };
        }


    }
}