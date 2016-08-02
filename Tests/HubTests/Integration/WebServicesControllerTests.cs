using System;
using Data.Interfaces;
using Fr8.Testing.Integration;
using HubTests.Fixtures;
using NUnit.Framework;
using StructureMap;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;

namespace HubTests.Integration
{
    [Explicit]
    [Category("HubTests.Integration")]
    public class WebServicesControllerTests : BaseHubIntegrationTest
    {
        public override string TerminalName
        {
            get { return "Hub"; }
        }

        [Test]
        public async Task Get_AddNewWebService_GetSavedWebService()
        {
            //Arrnage
            //create a new web service for integration test
            await CreateNewWebService();

            //Act
            string baseUrl = GetHubApiBaseUrl();
            var webServicesGetUrl = baseUrl + "WebServices/Get";
            var webServices = await HttpGetAsync<List<ActivityCategoryDTO>>(webServicesGetUrl);

            //Assert
            Assert.IsNotNull(webServices, "Web Services are not retrieved from HubWeb");
            Assert.IsTrue(webServices.Count > 0, "There are no web services in HubWeb");
            Assert.IsTrue(webServices.Any(ws => ws.Name.Equals("IntegrationTestWebService")), "The newly created web service is missing in HubWeb");


            CleanUpDatabase();
        }

        [Test]
        public async Task Post_PostNewWebService_CheckDatabaseForTheNewWebService()
        {
            var newWebService = await CreateNewWebService();

            //Assert
            Assert.IsNotNull(newWebService, "The new Web Service is not posted to HubWeb");
            Assert.IsTrue(newWebService.Id != Guid.Empty, "There is no new web services added in HubWeb");
            Assert.IsTrue(newWebService.Name.Equals("IntegrationTestWebService"), "The newly created web service is missing in HubWeb");

            CleanUpDatabase();
        }

        [Test]
        public async Task GetActivities_ShouldGetOneMonitorActivity()
        {
            //Arrange
            var activityCategoryId = ActivityCategories.MonitorId.ToString();

            //Act
            string baseUrl = GetHubApiBaseUrl();
            var webServicesActivitiesUrl = baseUrl + "WebServices";

            var webServiceActivities = await HttpGetAsync<List<WebServiceActivitySetDTO>>(webServicesActivitiesUrl + "?id=" + activityCategoryId);

            //Assert
            Assert.IsNotNull(webServiceActivities, "The activity sets are not retrieved from HubWeb");
            Assert.IsTrue(webServiceActivities.Count > 0, "There is no activity sets in HubWeb");
            Assert.IsTrue(webServiceActivities[0].Activities.Count > 0, "The activity sets are missing in HubWeb");
        }

        private void CleanUpDatabase()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var integrationTestWebServices = uow.ActivityCategoryRepository.GetQuery().Where(webService => webService.Name.Equals("IntegrationTestWebService"));

                foreach (var webService in integrationTestWebServices.ToList())
                {
                    uow.ActivityCategoryRepository.Remove(webService);
                }

                uow.SaveChanges();
            }
        }

        private async Task<ActivityCategoryDTO> CreateNewWebService()
        {
            var webServiceDTO = FixtureData.BasicWebServiceDTOWithoutId();

            //Act
            string baseUrl = GetHubApiBaseUrl();
            var webServicesPostUrl = baseUrl + "WebServices/Post";

            var newWebService = await HttpPostAsync<ActivityCategoryDTO, ActivityCategoryDTO>(webServicesPostUrl, webServiceDTO);

            return newWebService;
        }
    }
}
