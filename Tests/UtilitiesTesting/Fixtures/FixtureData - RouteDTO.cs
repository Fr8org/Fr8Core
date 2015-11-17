using Data.Interfaces.DataTransferObjects;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static RouteEmptyDTO CreateTestRouteDTO()
        {
            return new RouteEmptyDTO()
            {
                Name = "processtemplate1",
                Description = "Description for test process template",
                RouteState = 1
                //DockyardAccount = FixtureData.TestDockyardAccount1()
            };
        }

           
    }
}