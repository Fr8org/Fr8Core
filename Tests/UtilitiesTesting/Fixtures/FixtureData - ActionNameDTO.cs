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
        public static ActionNameDTO TestActionNameDTO1()
        {
            return new ActionNameDTO
            {
                ActionType = "Write SQL",
                Version = "1.0"
            };
        }
    }
}
