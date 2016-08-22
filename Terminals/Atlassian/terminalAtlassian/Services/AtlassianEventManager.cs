using System;
using Newtonsoft.Json;
using terminalAtlassian.Interfaces;
using System.Threading.Tasks;
using StructureMap;
using terminalAtlassian.Models;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.Managers;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using Newtonsoft.Json.Linq;
using Fr8.Infrastructure.Data.DataTransferObjects;
using AutoMapper;
using Atlassian.Jira;

namespace terminalAtlassian.Services
{
    public class AtlassianEventManager : IAtlassianEventManager
    {
        private readonly IAtlassianSubscriptionManager _atlassianSubscription;
        private readonly ICrateManager _crateManager;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public AtlassianEventManager(IAtlassianSubscriptionManager atlassianSubscription, ICrateManager crateManager)
        {
            _atlassianSubscription = atlassianSubscription;
            _crateManager = crateManager;
        }

        public async Task<Crate> ProcessExternalEvents(string curExternalEventPayload)
        {
            var issue = JsonConvert.DeserializeObject<Models.JiraIssueEvent>(curExternalEventPayload);
            var atlassianEventCM = new JiraIssueWithCustomFieldsCM
            {
               JiraIssue = new AtlassianIssueEvent
               {
                    IssueKey = issue.Issue.Key,
                    IssueId = issue.Issue.Id,
                    UserId = issue.User.Email,
                    Time = issue.Timestamp,
                    ChangedAspect = issue.Issue.Fields.Project.Name,
                    EventType = issue.IssueEventTypeName,
                    IssueEvent = new Fr8.Infrastructure.Data.Manifests.JiraIssueEvent
                    {
                        IssueAssigneeName = issue.Issue.Fields.Assignee.DisplayName,
                        IssueType = issue.Issue.Fields.IssueType.Name,
                        IssueAssigneeEmailAddress = issue.Issue.Fields.Assignee.EmailAddress,
                        IssuePriority = issue.Issue.Fields.Priority.Name,
                        IssueResolution = issue.Issue.Fields.Resolution,
                        IssueStatus = issue.Issue.Fields.Status.Name,
                        IssueSummary = issue.Issue.Fields.Summary,
                        ProjectName = issue.Issue.Fields.Project.Name,
                        Timestamp = issue.Timestamp,
                        UserName = issue.User.DisplayName,
                        WebhookEvent = issue.WebhookEvent,
                        Description = issue.Issue.Fields.Description
                    }
                },
                CustomFields = null
            };
            var eventReportContent = new EventReportCM
            {
                EventNames = string.Join(",", atlassianEventCM.JiraIssue.ChangedAspect),
                ExternalAccountId = atlassianEventCM.JiraIssue.UserId,
                EventPayload = new CrateStorage(Crate.FromContent("Atlassian Issue Event", atlassianEventCM)),
                Manufacturer = "Atlassian"
            };
            
            var curEventReport = Crate.FromContent("Atlassian Issue Event", eventReportContent);
            return curEventReport;
        }

        public async Task<Crate> ProcessInternalEvents(IContainer container, string curExternalEventPayload)
        {
            var curFr8UserAndToken = ConfirmAuthentication(curExternalEventPayload);
            var hubCommunicator = container.GetInstance<IHubCommunicator>();
            try
            {
                _atlassianSubscription.CreateConnect(hubCommunicator, curFr8UserAndToken.Item2);
            }
            catch
            {
                throw new ArgumentException("Webhook could not be created");
            }
            return null;

        }
        private Tuple<string, AuthorizationToken> ConfirmAuthentication(string curExternalEventPayload)
        {
            var jo = (JObject)JsonConvert.DeserializeObject(curExternalEventPayload);
            var curFr8UserId = jo["fr8_user_id"].Value<string>();
            var authToken = JsonConvert.DeserializeObject<AuthorizationTokenDTO>(jo["auth_token"].ToString());

            if (authToken == null)
            {
                throw new ArgumentException("Authorization Token required");
            }

            if (string.IsNullOrEmpty(curFr8UserId))
            {
                throw new ArgumentException("Fr8 User ID is not in the correct format.");
            }

            return new Tuple<string, AuthorizationToken>(curFr8UserId, Mapper.Map<AuthorizationToken>(authToken));
        }
    }
}
 