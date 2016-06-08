using fr8.Infrastructure.Data.DataTransferObjects;

namespace HubTests.Fixtures
{
    public partial class FixtureData
    {
        public static WebServiceDTO BasicWebServiceDTOWithoutId()
        {
            var webServiceDTO = new WebServiceDTO { Name = "IntegrationTestWebService", IconPath = "IntegrationTestIconPath" };

            return webServiceDTO;
        }
    }
}
