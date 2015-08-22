using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static ProcessNodeTemplateDO TestProcessNodeTemplateDO1()
        {
            ProcessNodeTemplateDO ProcessNodeTemplateDO = new ProcessNodeTemplateDO()
            {
                Id = 50,
                NodeTransitions = "[{'Flag':'x','Id':'1'},{'Flag':'x','Id':'2'}]"
            };
            return ProcessNodeTemplateDO;
        }
    }
}
