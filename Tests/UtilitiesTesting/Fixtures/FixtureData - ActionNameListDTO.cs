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
        public static ActivityNameListDTO TestActivityNameListDTO1()
        {
            return new ActivityNameListDTO{ ActivityNames = new List<ActivityNameDTO>(){FixtureData.TestActivityNameDTO1()}};
        }
    }
}
