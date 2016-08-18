using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using StructureMap;
using Fr8.Infrastructure.Communication;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Security;

namespace Fr8.Testing.Integration
{
    public abstract class BasePlanDirectoryIntegrationTest : BaseIntegrationTest
    {
        private HttpClient _httpClient;

        protected virtual string TestUserEmail => ConfigurationManager.AppSettings["TestUserAccountName"];

        protected virtual string TestUserPassword => ConfigurationManager.AppSettings["TestUserPassword"];

        public override string TerminalName => "PlanDirectory";

        public BasePlanDirectoryIntegrationTest()
        {
            ObjectFactory.Initialize();
            ObjectFactory.Configure(Hub.StructureMap.StructureMapBootStrapper.LiveConfiguration);
            ObjectFactory.Configure(Fr8.Infrastructure.StructureMap.StructureMapBootStrapper.LiveConfiguration);

            // Use a common HttpClient for all REST operations within testing session 
            // to ensure the presense of the authentication cookie. 
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = GetPlanDirectoryBaseUri();
            _httpClient.Timeout = TimeSpan.FromMinutes(2);

            Crate = new CrateManager();
            _baseUrl = GetPlanDirectoryBaseApiUrl();
            RestfulServiceClient = new RestfulServiceClient(_httpClient);

            // Get auth cookie from the Hub and save it to HttpClient's internal cookie storage
            AuthenticateWebApi(TestUserEmail, TestUserPassword).Wait();
        }

        private string GetPlanDirectoryBaseApiUrl()
        {
            return ConfigurationManager.AppSettings["HubApiBaseUrl"];            
        }

        private Uri GetPlanDirectoryBaseUri()
        {
            var hubApiBaseUrl = new Uri(GetPlanDirectoryBaseApiUrl());
            var hubBaseUrl = new Uri(hubApiBaseUrl.Scheme + "://" + hubApiBaseUrl.Host + ":" + hubApiBaseUrl.Port);
            return hubBaseUrl;
        }

        protected async Task AuthenticateWebApi(string email, string password)
        {
            try
            {
                var content = await HttpPostAsync<string, object>(
                    _baseUrl + string.Format(
                        "authentication/login?username={0}&password={1}",
                        Uri.EscapeDataString(email),
                        Uri.EscapeDataString(password)
                    ),
                    null
                );
                System.Diagnostics.Trace.WriteLine("Authenticated with PlanDirectory successfully.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Error during authentication with PlanDirectory: " + ex.Message);
            }
        }
    }
}
