using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;

namespace Fr8.Testing.Unit.Fixtures
{
    partial class FixtureData
    {
        public static PlanEmptyDTO CreateTestPlanDTO()
        {
            return new PlanEmptyDTO()
            {
                Name = "plan1",
                Description = "Description for test plan",
                PlanState = 1,
                Visibility = PlanVisibility.Standard
                //DockyardAccount = FixtureData.TestDockyardAccount1()
            };
        }
    }
}