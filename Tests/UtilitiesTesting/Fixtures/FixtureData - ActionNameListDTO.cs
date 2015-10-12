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
        public static ActionNameListDTO TestActionNameListDTO1()
        {
            return new ActionNameListDTO{ ActionNames = new List<ActionNameDTO>(){FixtureData.TestActionNameDTO1()}};
        }
    }
}
