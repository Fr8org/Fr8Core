using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Linq;
using NUnit.Framework;
using StructureMap;
using System.Net.Http;
using System.Net.Http.Formatting;
using Data.Interfaces;
using Fr8.Infrastructure.Communication;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Security;

namespace Fr8.Testing.Integration
{
    public abstract class BaseHubIntegrationTest : BaseIntegrationTest
    {
        private readonly HttpClient _httpClient;

        protected virtual string TestUserEmail => ConfigurationManager.AppSettings["TestUserAccountName"];

        protected virtual string TestUserPassword => ConfigurationManager.AppSettings["TestUserPassword"];

        protected string TestEmail;
        protected string TestEmailName;

        public CredentialsDTO GetDocuSignCredentials()
        {
            var creds = new CredentialsDTO
            {
                Username = ConfigurationManager.AppSettings["TestUserEmail"], 
                Password = ConfigurationManager.AppSettings["DocuSignApiPassword"], 
                IsDemoAccount = true
            };
            return creds;
        }

        protected BaseHubIntegrationTest()
        {
            ObjectFactory.Initialize();
            ObjectFactory.Configure(Hub.StructureMap.StructureMapBootStrapper.LiveConfiguration);
            ObjectFactory.Configure(Infrastructure.StructureMap.StructureMapBootStrapper.LiveConfiguration);

            // Use a common HttpClient for all REST operations within testing session 
            // to ensure the presense of the authentication cookie. 
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = GetHubBaseUrl();
            _httpClient.Timeout = TimeSpan.FromMinutes(2);

            Crate = new CrateManager();
            _baseUrl = GetHubApiBaseUrl();
            RestfulServiceClient = new RestfulServiceClient(_httpClient);

            // Get auth cookie from the Hub and save it to HttpClient's internal cookie storage
            LoginUser(TestUserEmail, TestUserPassword).Wait();

            // Initailize EmailAssert utility.
            TestEmail = ConfigurationManager.AppSettings["TestEmail"];
            TestEmailName = ConfigurationManager.AppSettings["TestEmail_Name"];
            string hostname = ConfigurationManager.AppSettings["TestEmail_Pop3Server"];
            int port = int.Parse(ConfigurationManager.AppSettings["TestEmail_Port"]);
            bool useSsl = ConfigurationManager.AppSettings["TestEmail_UseSsl"] == "true" ? true : false;
            string username = ConfigurationManager.AppSettings["TestEmail_Username"];
            string password = ConfigurationManager.AppSettings["TestEmail_Password"];
            EmailAssert.InitEmailAssert(TestEmail, hostname, port, useSsl, username, password);
        }

        protected Dictionary<string, string> GetFr8HubAuthorizationHeader(string terminalName,string terminalVersion, string userId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var terminal = uow.TerminalRepository.GetQuery().Single(x => x.Name == terminalName && x.Version == terminalVersion);
                var fr8Token = $"key={terminal.Secret}" + (string.IsNullOrEmpty(userId) ? "" : $", user={userId}");
                return new Dictionary<string, string>
                {
                    {System.Net.HttpRequestHeader.Authorization.ToString(), $"FR8-TOKEN {fr8Token}"}
                };
            }
        }

        
        protected Dictionary<string, string> GetFr8TerminalAuthorizationHeader(string terminalName, string terminalVersion, string userId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var terminal = uow.TerminalRepository.GetQuery().Single(x => x.Name == terminalName && x.Version == terminalVersion);
                return new Dictionary<string, string>
                {
                    {"Fr8HubCallbackSecret", terminal.Secret},
                    {"Fr8HubCallBackUrl", ConfigurationManager.AppSettings["DefaultHubUrl"]},
                    {"Fr8UserId", userId }
                };
            }
        }


        public string GetHubApiBaseUrl()
        {
            return ConfigurationManager.AppSettings["HubApiBaseUrl"];
        }

        protected async Task LoginUser(string email, string password)
        {
            // The functions below re using ASP.NET MVC endpoi9nts to authenticate the user. 
            // Since we cannot use them in the self-hosted mode, we use WebAPI based 
            // authentication instead. 

            // Get login page and extract request validation token
            //var antiFogeryToken = await GetVerificationToken(_httpClient);

            // Login user
            //await Authenticate(email, password, antiFogeryToken, _httpClient);
            try
            {
                await AuthenticateWebApi(email, password);
            }
            catch (Exception)
            {
            }
        }

        protected Task RevokeTokens()
        {
            return RevokeTokens(TerminalName);
        }

        protected async Task RevokeTokens(string terminalName)
        {
            var tokens = await HttpGetAsync<IEnumerable<AuthenticationTokenTerminalDTO>>(
                _baseUrl + "authentication/tokens"
            );

            var docusignTokens = tokens?.FirstOrDefault(x => x.Name == terminalName);
            if (docusignTokens != null)
            {
                foreach (var token in docusignTokens.AuthTokens)
                {
                    await HttpPostAsync<string>(_baseUrl + "authentication/tokens/revoke?id=" + token.Id, null);
                }
            }
        }

        protected Uri GetHubBaseUrl()
        {
            var hubApiBaseUrl = new Uri(GetHubApiBaseUrl());
            var hubBaseUrl = new Uri(hubApiBaseUrl.Scheme + "://" + hubApiBaseUrl.Host + ":" + hubApiBaseUrl.Port);
            return hubBaseUrl;
        }

        private async Task AuthenticateWebApi(string email, string password)
        {
            await HttpPostAsync<string, object>(_baseUrl + $"authentication/login?username={Uri.EscapeDataString(email)}&password={Uri.EscapeDataString(password)}", null);
        }

        public async Task<IncomingCratesDTO> GetRuntimeCrateDescriptionsFromUpstreamActivities(Guid curActivityId)
        {
            var url = $"{GetHubApiBaseUrl()}/plan_nodes/signals/?id={curActivityId}";
            return await HttpGetAsync<IncomingCratesDTO>(url);
        }
        protected async Task<Guid> ExtractTerminalDefaultToken(string terminalName)
        {
            var tokens = await HttpGetAsync<IEnumerable<AuthenticationTokenTerminalDTO>>(GetHubApiBaseUrl() + "manageauthtoken/");
            Assert.NotNull(tokens, "No authorization tokens were found for the integration testing user.");
            var terminal = tokens.FirstOrDefault(x => x.Name == terminalName);
            Assert.NotNull(terminal, $"No authorization tokens were found for the {terminalName}");
            var token = terminal.AuthTokens.FirstOrDefault(x => x.IsMain);
            Assert.NotNull(token, $"Authorization token for {terminalName} is not found for the integration testing user.Please go to the target instance of fr8 and log in with the integration testing user credentials.Then add a Google action to any plan and be sure to set the 'Use for all Activities' checkbox on the Authorize Accounts dialog while authenticating");

            return token.Id;
        }

        protected async Task<ActivityDTO> ConfigureActivity(ActivityDTO activity)
        {
            activity = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/configure", activity);

            return activity;
        }

        protected async Task<PayloadDTO> ExtractContainerPayload(ContainerDTO container)
        {
            var payload = await HttpGetAsync<PayloadDTO>(
                _baseUrl + "containers/payload?id=" + container.Id
            );

            return payload;
        }

        protected async Task<ContainerDTO> ExecutePlan(PlanDTO plan)
        {
            var container = await HttpPostAsync<string, ContainerDTO>(
                _baseUrl + "plans/run?planId=" + plan.Id,
                null
            );

            return container;
        }

        protected async Task SaveActivity(ActivityDTO activity)
        {
            await HttpPostAsync<ActivityDTO, ActivityDTO>(
                _baseUrl + "activities/save",
                activity
            );
        }
    }
}
