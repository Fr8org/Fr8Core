using Fr8.Infrastructure.Data.DataTransferObjects;

namespace HubTests.Fixtures
{
    public partial class FixtureData
    {
        public static ActivityCategoryDTO BasicWebServiceDTOWithoutId()
        {
            var webServiceDTO = new ActivityCategoryDTO
            {
                Name = "IntegrationTestWebService",
                IconPath = "IntegrationTestIconPath"
            };

            return webServiceDTO;
        }
    }
}
