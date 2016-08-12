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
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalAtlassian.Services
{
    public class AtlassianSubscriptionManager : IAtlassianSubscriptionManager
    {
        private readonly ICrateManager _crateManager;
        private readonly IHubEventReporter _eventReporter;
        private readonly IRestfulServiceClient _client;

        private readonly string DevConnectName = "(dev) Fr8 Company Jira integration";
        private readonly string DemoConnectName = "(demo) Fr8 Company Jira integration";
        private readonly string ProdConnectName = "Fr8 Company Jira integration";
        private readonly string TemporaryConnectName = "Fr8 Jira Integration Test";

        private readonly string debugUrl = "http://localhost:39768";
        private readonly string prodUrl = "https://terminalAtlassian.fr8.co";
        private readonly string devUrl = "http://dev-terminals.fr8.co:39768";
        private readonly string demoUrl = "http://demo-terminals.fr8.co:39768";
        private readonly string callbackUrl = CloudConfigurationManager.GetSetting("terminalAtlassian.CallbackUrl");

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

        //only create a connect when running on dev/production
        public async void CreateConnect(IHubCommunicator hubCommunicator, AuthorizationToken authToken)
        {
            string[] events = new string[4];
            events[0] = "jira:issue_created";
            events[1] = "jira:issue_updated";
            events[2] = "jira:issue_deleted";
            events[3] = "jira:worklog_updated";
            var post = new JiraSubscriptionPost();

            post.Events = events;
            post.ExcludeIssueDetails = false;
            post.JqlFilter = "";
            post.Url = callbackUrl;

            if (callbackUrl.Contains(prodUrl)) {
                post.Name = ProdConnectName;
            }
            else if(callbackUrl.Contains(devUrl)) {
                post.Name = DevConnectName;
            }
            else if (callbackUrl.Contains(demoUrl)) {
                post.Name = DemoConnectName;
            }
            else
            {
                post.Name = TemporaryConnectName;
            }
            var url = "/rest/webhooks/1.0/webhook";
            Jira jira = CreateRestClient(authToken.Token);
            var getSubscriptions = await jira.RestClient.ExecuteRequestAsync(Method.GET, url, post);
            var oldSubscriptions = getSubscriptions.ToList<JToken>();

            var subscriptionIsExists = false;
            for (var i = 0; i < oldSubscriptions.Count; i++)
            {
                if(oldSubscriptions[i].Value<string>("name") == post.Name)
                {
                    subscriptionIsExists = true;
                }
            }
            if (!subscriptionIsExists)
            {
                var subscription = await jira.RestClient.ExecuteRequestAsync(Method.POST, url, post);
            }
        }

        public void CreateOrUpdatePolling(IHubCommunicator hubCommunicator, AuthorizationToken authToken)
        {
        }

    }
}
 