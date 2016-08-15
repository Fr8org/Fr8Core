using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;

namespace Fr8.Testing.Unit.Fixtures
{
    partial class FixtureData
    {
        public static PlanNoChildrenDTO CreateTestPlanDTO(string planName = "")
        {
            return new PlanNoChildrenDTO()
            {
                Name = string.IsNullOrEmpty(planName) ? "plan1" : planName,
                Description = "Description for test plan",
                PlanState = "Inactive",
                Visibility = new PlanVisibilityDTO() { Hidden = false }
                //DockyardAccount = FixtureData.TestDockyardAccount1()
            };
        }
    }
}