using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static ActivityNameDTO TestActivityNameDTO1()
        {
            return new ActivityNameDTO
            {
                Name = "Write SQL",
                Version = "1.0"
            };
        }
    }
}
