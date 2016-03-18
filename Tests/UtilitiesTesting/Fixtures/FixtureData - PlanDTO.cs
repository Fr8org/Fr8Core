using Data.Interfaces.DataTransferObjects;

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
                Visibility = Data.States.PlanVisibility.Standard
                //DockyardAccount = FixtureData.TestDockyardAccount1()
            };
        }


    }
}