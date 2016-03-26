using Data.Entities;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static SubPlanDO TestSubPlanDO1()
        {
            var SubPlanDO = new SubPlanDO
            {
                Id = GetTestGuidById(50),
                NodeTransitions =
                    "[{'TransitionKey':'true','ProcessNodeId':'1'},{'TransitionKey':'false','ProcessNodeId':'2'}]"
            };
            return SubPlanDO;
        }

        public static SubPlanDO TestSubPlanHealthDemo()
        {
            var SubPlanDO = new SubPlanDO
            {
                Id = GetTestGuidById(50),
                ParentPlanNodeId = GetTestGuidById(23),
                RootPlanNodeId = GetTestGuidById(23),
                NodeTransitions = "[{'TransitionKey':'true','ProcessNodeId':'2'}]"
            };
            return SubPlanDO;
        }

        public static SubPlanDO TestSubPlanDO2()
        {
            SubPlanDO SubPlanDO = new SubPlanDO()
            {
                Id = GetTestGuidById(51),
                Name = "TestName",
                NodeTransitions =
                    "[{'TransitionKey':'true','ProcessNodeId':'1'},{'TransitionKey':'false','ProcessNodeId':'2'}]",
                ParentPlanNodeId = GetTestGuidById(50),
                RootPlanNodeId = GetTestGuidById(50),
                StartingSubPlan = true
            };
            return SubPlanDO;
        }

        public static SubPlanDO TestSubPlanDO3()
        {
            SubPlanDO SubPlanDO = new SubPlanDO()
            {
                Id = GetTestGuidById(50),
                NodeTransitions =
                    "[{'TransitionKey':'true','ProcessNodeId':'3'},{'TransitionKey':'false','ProcessNodeId':'5'}]"
            };
            return SubPlanDO;
        }

        public static SubPlanDO TestSubPlanDO4()
        {
            SubPlanDO SubPlanDO = new SubPlanDO()
            {
                Id = GetTestGuidById(1),
                NodeTransitions =
                    "[{'TransitionKey':'true','ProcessNodeId':'3'},{'TransitionKey':'false','ProcessNodeId':'5'}]"
            };
            return SubPlanDO;
        }
    }
}