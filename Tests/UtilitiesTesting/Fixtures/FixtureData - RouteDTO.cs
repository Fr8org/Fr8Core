using Data.Interfaces.DataTransferObjects;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static RouteEmptyDTO CreateTestRouteDTO()
        {
            return new RouteEmptyDTO()
            {
                Name = "route1",
                Description = "Description for test plan",
                RouteState = 1,
                Visibility = Data.States.PlanVisibility.Standard
                //DockyardAccount = FixtureData.TestDockyardAccount1()
            };
        }

           
    }
}