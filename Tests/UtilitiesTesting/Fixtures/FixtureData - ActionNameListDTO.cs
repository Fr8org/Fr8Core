using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;

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
