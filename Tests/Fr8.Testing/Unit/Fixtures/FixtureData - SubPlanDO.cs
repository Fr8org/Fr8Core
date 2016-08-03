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
            };
            return SubPlanDO;
        }

        public static SubplanDO TestSubPlanDO2()
        {
            SubplanDO subplanDO = new SubplanDO()
            {
                Id = GetTestGuidById(51),
                Name = "TestName",
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
            };
            return subplanDO;
        }

        public static SubplanDO TestSubPlanDO4()
        {
            SubplanDO subplanDO = new SubplanDO()
            {
                Id = GetTestGuidById(1),
            };
            return subplanDO;
        }
    }
}