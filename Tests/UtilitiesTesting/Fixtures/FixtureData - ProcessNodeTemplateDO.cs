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
                NodeTransitions = "[{'TransitionKey':'x','ProcessNodeId':'1'},{'TransitionKey':'x','ProcessNodeId':'2'}]"
            };
            return ProcessNodeTemplateDO;
        }

        public static ProcessNodeTemplateDO TestProcessNodeTemplateDO2()
        {
            ProcessNodeTemplateDO ProcessNodeTemplateDO = new ProcessNodeTemplateDO()
            {
                Id = 50,
                NodeTransitions = "[{'TransitionKey':'true','ProcessNodeId':'1'},{'TransitionKey':'false','ProcessNodeId':'2'}]"
            };
            return ProcessNodeTemplateDO;
        }

        public static ProcessNodeTemplateDO TestProcessNodeTemplateDO3()
        {
            ProcessNodeTemplateDO ProcessNodeTemplateDO = new ProcessNodeTemplateDO()
            {
                Id = 50,
                NodeTransitions = "[{'TransitionKey':'true','ProcessNodeId':'3'},{'TransitionKey':'false','ProcessNodeId':'5'}]"
            };
            return ProcessNodeTemplateDO;
        }
    }
}
