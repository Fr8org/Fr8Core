using System.Linq;
using Atlassian.Jira;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Fr8.TerminalBase.Models;
using Newtonsoft.Json;
using terminalAtlassian.Interfaces;
using terminalAtlassian.Helpers;
using terminalAtlassian.Models;
using Fr8.Infrastructure.Data.Managers;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Services;
using Newtonsoft.Json.Linq;
using RestSharp;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalAtlassian.Services
{
    public class AtlassianSubscriptionManager : IAtlassianSubscriptionManager
    {
        private readonly ICrateManager _crateManager;
        private readonly IHubEventReporter _eventReporter;
        private readonly IRestfulServiceClient _client;

        private string terminalEndpoint = CloudConfigurationManager.GetSetting("terminalAtlassian.TerminalEndpoint");

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
            post.Url = terminalEndpoint + "/terminals/terminalatlassian/process-issue";
            post.Name = "Fr8 Jira Integration for (" + terminalEndpoint + ")";

            var url = "/rest/webhooks/1.0/webhook";
            Jira jira = CreateRestClient(authToken.Token);
            var getSubscriptions = await jira.RestClient.ExecuteRequestAsync(Method.GET, url, post);
            var oldSubscriptions = getSubscriptions.ToList<JToken>();

            var subscriptionDoesExists = false;
            for (var i = 0; i < oldSubscriptions.Count; i++)
            {
                if(oldSubscriptions[i].Value<string>("name") == post.Name)
                {
                    subscriptionDoesExists = true;
                }
            }
            if (!subscriptionDoesExists)
            {
                var subscription = await jira.RestClient.ExecuteRequestAsync(Method.POST, url, post);
            }
        }
    }
}
 