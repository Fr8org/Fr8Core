using Data.Entities;

namespace Fr8.Testing.Unit.Fixtures
{
    public partial class FixtureData
    {
        public static SubplanDO TestSubPlanDO1()
        {
            var SubPlanDO = new SubplanDO
            {
                Id = GetTestGuidById(50),
                NodeTransitions =
                    "[{'TransitionKey':'true','ProcessNodeId':'1'},{'TransitionKey':'false','ProcessNodeId':'2'}]"
            };
            return SubPlanDO;
        }

        public static SubplanDO TestSubPlanHealthDemo()
        {
            var SubPlanDO = new SubplanDO
            {
                Id = GetTestGuidById(50),
                ParentPlanNodeId = GetTestGuidById(23),
                RootPlanNodeId = GetTestGuidById(23),
                NodeTransitions = "[{'TransitionKey':'true','ProcessNodeId':'2'}]"
            };
            return SubPlanDO;
        }

        public static SubplanDO TestSubPlanDO2()
        {
            SubplanDO subplanDO = new SubplanDO()
            {
                Id = GetTestGuidById(51),
                Name = "TestName",
                NodeTransitions =
                    "[{'TransitionKey':'true','ProcessNodeId':'1'},{'TransitionKey':'false','ProcessNodeId':'2'}]",
                ParentPlanNodeId = GetTestGuidById(50),
                RootPlanNodeId = GetTestGuidById(50),
                StartingSubPlan = true
            };
            return subplanDO;
        }

        public static SubplanDO TestSubPlanDO3()
        {
            SubplanDO subplanDO = new SubplanDO()
            {
                Id = GetTestGuidById(50),
                NodeTransitions =
                    "[{'TransitionKey':'true','ProcessNodeId':'3'},{'TransitionKey':'false','ProcessNodeId':'5'}]"
            };
            return subplanDO;
        }

        public static SubplanDO TestSubPlanDO4()
        {
            SubplanDO subplanDO = new SubplanDO()
            {
                Id = GetTestGuidById(1),
                NodeTransitions =
                    "[{'TransitionKey':'true','ProcessNodeId':'3'},{'TransitionKey':'false','ProcessNodeId':'5'}]"
            };
            return subplanDO;
        }
    }
}