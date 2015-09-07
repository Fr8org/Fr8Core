using Data.Entities;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static ProcessNodeTemplateDO TestProcessNodeTemplateDO1()
        {
            var ProcessNodeTemplateDO = new ProcessNodeTemplateDO
            {
                Id = 50,
                NodeTransitions =
                    "[{'TransitionKey':'true','ProcessNodeId':'1'},{'TransitionKey':'false','ProcessNodeId':'2'}]"
            };
            return ProcessNodeTemplateDO;
        }

        public static ProcessNodeTemplateDO TestProcessNodeTemplateHealthDemo()
        {
            var ProcessNodeTemplateDO = new ProcessNodeTemplateDO
            {
                Id = 50,
                ParentTemplateId = 23,
                NodeTransitions = "[{'TransitionKey':'true','ProcessNodeId':'2'}]"
            };
            return ProcessNodeTemplateDO;
        }

        public static ProcessNodeTemplateDO TestProcessNodeTemplateDO2()
        {
            ProcessNodeTemplateDO ProcessNodeTemplateDO = new ProcessNodeTemplateDO()
            {
                Id = 50,
                Name = "TestName",
                NodeTransitions =
                    "[{'TransitionKey':'true','ProcessNodeId':'1'},{'TransitionKey':'false','ProcessNodeId':'2'}]",
                ParentTemplateId = 50
            };
            return ProcessNodeTemplateDO;
        }

        public static ProcessNodeTemplateDO TestProcessNodeTemplateDO3()
        {
            ProcessNodeTemplateDO ProcessNodeTemplateDO = new ProcessNodeTemplateDO()
            {
                Id = 50,
                NodeTransitions =
                    "[{'TransitionKey':'true','ProcessNodeId':'3'},{'TransitionKey':'false','ProcessNodeId':'5'}]"
            };
            return ProcessNodeTemplateDO;
        }

        public static ProcessNodeTemplateDO TestProcessNodeTemplateDO4()
        {
            ProcessNodeTemplateDO ProcessNodeTemplateDO = new ProcessNodeTemplateDO()
            {
                Id = 1,
                NodeTransitions =
                    "[{'TransitionKey':'true','ProcessNodeId':'3'},{'TransitionKey':'false','ProcessNodeId':'5'}]"
            };
            return ProcessNodeTemplateDO;
        }
    }
}