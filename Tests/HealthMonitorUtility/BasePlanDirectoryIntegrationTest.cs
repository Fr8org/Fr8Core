using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using StructureMap;
using Fr8Data.Managers;
using Fr8Infrastructure.Communication;
using Fr8Infrastructure.Security;

namespace HealthMonitor.Utility
{
    public abstract class BasePlanDirectoryIntegrationTest : BaseIntegrationTest
    {
        private HttpClient _httpClient;

        protected virtual string TestUserEmail
        {
            get { return "integration_test_runner@fr8.company"; }
        }

        protected virtual string TestUserPassword
        {
            get { return "fr8#s@lt!"; }
        }

        public override string TerminalName
        {
            get { return "PlanDirectory"; }
        }

        public BasePlanDirectoryIntegrationTest()
        {
            ObjectFactory.Initialize();
            ObjectFactory.Configure(Hub.StructureMap.StructureMapBootStrapper.LiveConfiguration);
            ObjectFactory.Configure(Fr8Infrastructure.StructureMap.StructureMapBootStrapper.LiveConfiguration);

            // Use a common HttpClient for all REST operations within testing session 
            // to ensure the presense of the authentication cookie. 
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = GetPlanDirectoryBaseUri();
            _httpClient.Timeout = TimeSpan.FromMinutes(2);

            Crate = new CrateManager();
            _hmacService = new Fr8HMACService();
            _baseUrl = GetPlanDirectoryBaseApiUrl();
            RestfulServiceClient = new RestfulServiceClient(_httpClient);

            // Get auth cookie from the Hub and save it to HttpClient's internal cookie storage
            AuthenticateWebApi(TestUserEmail, TestUserPassword).Wait();
        }

        private string GetPlanDirectoryBaseApiUrl()
        {
            return ConfigurationManager.AppSettings["PlanDirectoryBaseApiUrl"];
        }

        private Uri GetPlanDirectoryBaseUri()
        {
            var hubApiBaseUrl = new Uri(GetPlanDirectoryBaseApiUrl());
            var hubBaseUrl = new Uri(hubApiBaseUrl.Scheme + "://" + hubApiBaseUrl.Host + ":" + hubApiBaseUrl.Port);
            return hubBaseUrl;
        }

        private async Task AuthenticateWebApi(string email, string password)
        {
            var content = await HttpPostAsync<string, object>(
                _baseUrl + string.Format(
                    "authentication/login?username={0}&password={1}",
                    Uri.EscapeDataString(email),
                    Uri.EscapeDataString(password)
                ),
                null
            );
        }
    }
}
