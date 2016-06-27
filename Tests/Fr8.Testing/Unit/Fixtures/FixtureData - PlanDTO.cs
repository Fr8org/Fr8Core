using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;

namespace Fr8.Testing.Unit.Fixtures
{
    partial class FixtureData
    {
        public static PlanEmptyDTO CreateTestPlanDTO(string planName = "")
        {
            return new PlanEmptyDTO()
            {
                Name = string.IsNullOrEmpty(planName) ? "plan1" : planName,
                Description = "Description for test plan",
                PlanState = 1,
                Visibility = PlanVisibility.Standard
                //DockyardAccount = FixtureData.TestDockyardAccount1()
            };
        }
    }
}