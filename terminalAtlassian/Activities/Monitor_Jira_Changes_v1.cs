using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Services;
using Newtonsoft.Json;
using terminalAtlassian.Interfaces;
using terminalAtlassian.Helpers;
using Atlassian.Jira;

namespace terminalAtlassian.Actions
{
    public class Monitor_Jira_Changes_v1 : TerminalActivity<Monitor_Jira_Changes_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("29d1ce42-252b-4152-8ec3-b55a2095e8b1"),
            Version = "1",
            Name = "Monitor_Jira_Changes_v1",
            Label = "Monitor Jira Changes",
            NeedsAuthentication = true,
            MinPaneWidth = 330,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[] {
                ActivityCategories.Monitor,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        private const string RuntimeCrateLabel = "Monitor Atlassian Runtime Fields";
        private const string EventSubscriptionsCrateLabel = "Atlassian Issue Event";

        private const string IssueKey = "Issue Key";
        private const string ProjectName = "Project Name";
        private const string IssueResolution = "Issue Resolution";
        private const string IssuePriority = "Issue Priority";
        private const string IssueAssignee = "Issue Assignee Name";
        private const string IssueSummary = "Issue Summary";
        private const string IssueStatus = "Issue Status";
        private const string IssueDescription = "Issue Description";
        private const string EventType = "Event Type";

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextBlock Description { get; set; }

            public DropDownList ProjectSelector { get; set; }

            public ActivityUi()
            {
                Description = new TextBlock()
                {
                    Value = "This activity will monitor when an issue is created or updated",
                    Label = "Description",
                    Name = nameof(Description)
                };
                ProjectSelector = new DropDownList
                {
                    Label = "Select Jira Project",
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };
                Controls = new List<ControlDefinitionDTO> { Description, ProjectSelector };
            }

        };

        private readonly IAtlassianService _atlassianService;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IAtlassianEventManager _atlassianEventManager;

        public override Task FollowUp()
        {
            if(EventSubscriptions.Subscriptions != null)
            {
                EventSubscriptions.Subscriptions.Clear();
            }

            EventSubscriptions.Manufacturer = "Atlassian";
            EventSubscriptions.Add(ActivityUI.ProjectSelector.selectedKey);

            return Task.FromResult(0);
        }

        public Monitor_Jira_Changes_v1(ICrateManager crateManager, IAtlassianService atlassianService, IPushNotificationService pushNotificationService, IAtlassianEventManager atlassianEventManager)
            : base(crateManager)
        {
            _atlassianService = atlassianService;
            _pushNotificationService = pushNotificationService;
            _atlassianEventManager = atlassianEventManager;
        }

        public override async Task Initialize()
        {
            ActivityUI.ProjectSelector.ListItems = _atlassianService
            .GetProjects(AuthorizationToken)
            .ToListItems()
            .ToList();

            CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(RuntimeCrateLabel)
                                .AddField(ProjectName)
                                .AddField(IssueResolution)
                                .AddField(IssuePriority)
                                .AddField(IssueAssignee)
                                .AddField(IssueSummary)
                                .AddField(IssueStatus)
                                .AddField(IssueDescription)
                                .AddField(IssueKey)
                                .AddField(EventType);

            CrateSignaller.MarkAvailableAtRuntime<JiraIssueWithCustomFieldsCM>(RuntimeCrateLabel);
        }
        public override Task Activate()
        {
            return Task.FromResult(0);
        }

        protected override Task Validate()
        {
            if (string.IsNullOrEmpty(ActivityUI.ProjectSelector.Value))
            {
                ValidationManager.SetError("Project is not specified", ActivityUI.ProjectSelector);
            }

            return Task.FromResult(0);
        }

        public override async Task Run()
        {
            var eventCrate = Payload.CrateContentsOfType<EventReportCM>(x => x.Label == "Atlassian Issue Event").FirstOrDefault();
            if (eventCrate == null)
            {
                RequestPlanExecutionTermination("Atlassian event payload was not found");
                return;
            }

            var atlassianEventPayload = eventCrate.EventPayload.CrateContentsOfType<JiraIssueWithCustomFieldsCM>()
                    .FirstOrDefault(e => e.JiraIssue.ChangedAspect.Contains(ActivityUI.ProjectSelector.selectedKey));

            if (atlassianEventPayload == null)
            {
                RequestPlanExecutionTermination("Atlassian event payload was not found");
                return;
            }
            var jiraIssueWithCustomFields = atlassianEventPayload;
            JiraIssueWithCustomFieldsCM jiraIssueWithCustomFieldsCM = new JiraIssueWithCustomFieldsCM();
            jiraIssueWithCustomFieldsCM.JiraIssue = jiraIssueWithCustomFields.JiraIssue;
            var jira = CreateJiraRestClient();
            var issue = jira.GetIssue(jiraIssueWithCustomFields.JiraIssue.IssueKey);

            var jiraComments = issue.GetComments();

            // add custom fields to the manifest
            var customFields = new JiraCustomField[issue.CustomFields.Count];
            for(var i = 0; i < issue.CustomFields.Count; i++)
            {
                var customField = new JiraCustomField();
                customField.Key = issue.CustomFields[i].Name;
                for(var j = 0; j < issue.CustomFields[i].Values.Length; j++)
                {
                    customField.Values = new string[issue.CustomFields[i].Values.Length];
                    customField.Values[j] = issue.CustomFields[i].Values[j];
                }
                customFields[i] = customField;
            }

            // add comments to the manifest
            var comments = new JiraComment[jiraComments.Count];
            for(var i = 0; i < jiraComments.Count; i++)
            {
                var comment = new JiraComment();
                comment.Author = jiraComments[i].Author;
                comment.Body = jiraComments[i].Body;
                comment.CreatedDate = jiraComments[i].CreatedDate;
                comment.GroupLevel = jiraComments[i].GroupLevel;
                comment.Id = jiraComments[i].Id;
                comment.RoleLevel = jiraComments[i].RoleLevel;
                comment.UpdateAuthor = jiraComments[i].UpdateAuthor;
                comment.UpdatedDate = jiraComments[i].UpdatedDate;
                comments[i] = comment;

            }
            jiraIssueWithCustomFieldsCM.CustomFields = customFields;
            jiraIssueWithCustomFieldsCM.Comments = comments;

            // payload in the type of JiraIssueWithCustomFieldsCM (that includes custom fields of the issue and comments)
            Payload.Add(Crate<JiraIssueWithCustomFieldsCM>.FromContent(RuntimeCrateLabel,jiraIssueWithCustomFieldsCM));

            // payload in the type of StandardPayloadDataCM (does not include custom fields and comments)
            Payload.Add(Crate<StandardPayloadDataCM>.FromContent(RuntimeCrateLabel, new StandardPayloadDataCM(
                                                                  new KeyValueDTO(IssueKey, jiraIssueWithCustomFields.JiraIssue.IssueKey),
                                                                  new KeyValueDTO(ProjectName, jiraIssueWithCustomFields.JiraIssue.IssueEvent.ProjectName),
                                                                  new KeyValueDTO(IssueResolution, jiraIssueWithCustomFields.JiraIssue.IssueEvent.IssueResolution),
                                                                  new KeyValueDTO(IssuePriority, jiraIssueWithCustomFields.JiraIssue.IssueEvent.IssuePriority),
                                                                  new KeyValueDTO(IssueAssignee, jiraIssueWithCustomFields.JiraIssue.IssueEvent.IssueAssigneeName),
                                                                  new KeyValueDTO(IssueSummary, jiraIssueWithCustomFields.JiraIssue.IssueEvent.IssueSummary),
                                                                  new KeyValueDTO(IssueStatus, jiraIssueWithCustomFields.JiraIssue.IssueEvent.IssueStatus),
                                                                  new KeyValueDTO(IssueDescription, jiraIssueWithCustomFields.JiraIssue.IssueEvent.Description),
                                                                  new KeyValueDTO(IssueKey, jiraIssueWithCustomFields.JiraIssue.IssueKey),
                                                                  new KeyValueDTO(EventType, jiraIssueWithCustomFields.JiraIssue.EventType)
                                                                  )));
        }
        public IEnumerable<CustomField> GetCustomFields()
        {
            var jira = CreateJiraRestClient();
            var customFields = jira.GetCustomFields();
            return customFields;
        }

        public Jira CreateJiraRestClient()
        {
            var credentialsDTO = JsonConvert.DeserializeObject<CredentialsDTO>(AuthorizationToken.Token).EnforceDomainSchema();
            Jira jira = Jira.CreateRestClient(credentialsDTO.Domain, credentialsDTO.Username, credentialsDTO.Password);
            return jira;
        }

    }
    
}
