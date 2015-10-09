using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesTesting.Fixtures
{

    public static class FixtureData_MTOobjects
    {
        public static MT_Organization TestOrganization()
        {
            return new MT_Organization()
            {
                Name = "testOrganization"
            };
        }


    }

}
