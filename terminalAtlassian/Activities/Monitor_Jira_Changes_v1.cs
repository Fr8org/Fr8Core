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
                                            .AddField(IssueKey);
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

            var atlassianEventPayload = eventCrate.EventPayload.CrateContentsOfType<AtlassianIssueEventCM>()
                    .FirstOrDefault(e => e.ChangedAspect.Contains(ActivityUI.ProjectSelector.selectedKey));

            if (atlassianEventPayload == null)
            {
                RequestPlanExecutionTermination("Atlassian event payload was not found");
                return;
            }
            var jiraIssue = atlassianEventPayload;
            Payload.Add(Crate<StandardPayloadDataCM>.FromContent(RuntimeCrateLabel, new StandardPayloadDataCM(
                                                                    new KeyValueDTO(IssueKey, jiraIssue.IssueKey),
                                                                    new KeyValueDTO(ProjectName, jiraIssue.IssueEvent.ProjectName),
                                                                    new KeyValueDTO(IssueResolution, jiraIssue.IssueEvent.IssueResolution),
                                                                    new KeyValueDTO(IssuePriority, jiraIssue.IssueEvent.IssuePriority),
                                                                    new KeyValueDTO(IssueAssignee, jiraIssue.IssueEvent.IssueAssigneeName),
                                                                    new KeyValueDTO(IssueSummary, jiraIssue.IssueEvent.IssueSummary),
                                                                    new KeyValueDTO(IssueStatus, jiraIssue.IssueEvent.IssueStatus),
                                                                    new KeyValueDTO(IssueDescription, jiraIssue.IssueEvent.Description),
                                                                    new KeyValueDTO(IssueKey, jiraIssue.IssueKey)
                                                                    )));
            }
    }
}
