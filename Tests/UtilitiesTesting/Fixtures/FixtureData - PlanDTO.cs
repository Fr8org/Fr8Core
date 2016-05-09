using Fr8Data.DataTransferObjects;

namespace UtilitiesTesting.Fixtures
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
                Visibility = Fr8Data.States.PlanVisibility.Standard
                //DockyardAccount = FixtureData.TestDockyardAccount1()
            };
        }


    }
}