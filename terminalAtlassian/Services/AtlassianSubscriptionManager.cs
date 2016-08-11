using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Atlassian.Jira;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Fr8.TerminalBase.Errors;
using Fr8.TerminalBase.Models;
using Newtonsoft.Json;
using terminalAtlassian.Interfaces;
using terminalAtlassian.Helpers;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using StructureMap;
using terminalAtlassian.Models;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.Managers;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Services;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;
using RestSharp;

namespace terminalAtlassian.Services
{
    public class AtlassianSubscriptionManager : IAtlassianSubscriptionManager
    {
        private readonly ICrateManager _crateManager;
        private readonly IHubEventReporter _eventReporter;
        private readonly IRestfulServiceClient _client;

        private readonly string DevConnectName = "(dev) Fr8 Company DocuSign integration";
        private readonly string DemoConnectName = "(demo) Fr8 Company DocuSign integration";
        private readonly string ProdConnectName = "Fr8 Company DocuSign integration";
        private readonly string TemporaryConnectName = "int-tests-Fr8";

        private Jira CreateRestClient(string token)
        {
            var credentialsDTO = JsonConvert.DeserializeObject<CredentialsDTO>(token).EnforceDomainSchema();
            return Jira.CreateRestClient(credentialsDTO.Domain, credentialsDTO.Username, credentialsDTO.Password);
        }

        public AtlassianSubscriptionManager(ICrateManager crateManager,  IHubEventReporter eventReporter, IRestfulServiceClient client)
        {
            _client = client;
            _crateManager = crateManager;
            _eventReporter = eventReporter;
        }

       
        public async Task CreatePlan_MonitorAllJiraEvents(IHubCommunicator hubCommunicator, AuthorizationToken authToken)
        {
        }

        //only create a connect when running on dev/production
        public async void CreateConnect(IHubCommunicator hubCommunicator, AuthorizationToken authToken)
        {
            string[] events = new string[2];
            events[0] = "jira:issue_created";
            events[1] = "jira:issue_updated";
            var post = new JiraSubscriptionPost();

            post.Events = events;
            post.ExcludeIssueDetails = false;
            post.JqlFilter = "";
            post.Url = "https://6183552a.ngrok.io/terminals/terminalatlassian/subscribe";
            post.Name = "test";

            var url = "https://maginot.atlassian.net/rest/webhooks/1.0/webhook";
            try
            {
                Jira jira = CreateRestClient(authToken.Token);
                var subscription = await jira.RestClient.ExecuteRequestAsync(Method.POST, url, post);
            }catch(Exception ex)
            {
                var x = 0;
            }
        }

        public void CreateOrUpdatePolling(IHubCommunicator hubCommunicator, AuthorizationToken authToken)
        {
        }

        public Task CreatePlan_MonitorAllDocuSignEvents(IHubCommunicator hubCommunicator, AuthorizationToken authToken)
        {
            throw new NotImplementedException();
        }
    }
}
 