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
using System.Diagnostics;
using Fr8.TerminalBase.Models;
using Newtonsoft.Json.Linq;
using Fr8.Infrastructure.Data.DataTransferObjects;
using AutoMapper;

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
            var issue = JsonConvert.DeserializeObject<JiraIssueEvent>(curExternalEventPayload);
            var atlassianEventCM = new AtlassianIssueEventCM
            {
                IssueKey = issue.Issue.Key,
                IssueId = issue.Issue.Id,
                UserId = issue.User.Email,
                Time = issue.Timestamp,
                ChangedAspect = issue.IssueEventTypeName,
                IssueEvent = new JiraIssueEventCM
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
                    WebhookEvent = issue.WebhookEvent
                }
                
            };
            var eventReportContent = new EventReportCM
            {
                EventNames = string.Join(",", atlassianEventCM.ChangedAspect),
                ExternalAccountId = atlassianEventCM.UserId,
                EventPayload = new CrateStorage(Crate.FromContent("Atlassian Issue Event", atlassianEventCM)),
                Manufacturer = "Atlassian"
            };
            
            ////prepare the event report
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
                _atlassianSubscription.CreateOrUpdatePolling(hubCommunicator, curFr8UserAndToken.Item2);
            }
            finally
            {
                await _atlassianSubscription.CreatePlan_MonitorAllDocuSignEvents(hubCommunicator, curFr8UserAndToken.Item2);
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

        public Task<Crate> ProcessExternalEvents(IContainer container, string curExternalEventPayload)
        {
            throw new NotImplementedException();
        }
    }
}
 