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
                Description = "Description for test route",
                RouteState = 1
                //DockyardAccount = FixtureData.TestDockyardAccount1()
            };
        }

           
    }
}