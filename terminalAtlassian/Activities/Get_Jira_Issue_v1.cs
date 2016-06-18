using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using StructureMap;
using terminalAtlassian.Services;

namespace terminalAtlassian.Actions
{
    public class Get_Jira_Issue_v1 : TerminalActivity<Get_Jira_Issue_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Version = "1",
            Name = "Get_Jira_Issue",
            Label = "Get Jira Issue",
            NeedsAuthentication = true,
            Category = ActivityCategory.Receivers,
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextSource IssueNumber { get; set; }

            public ActivityUi()
            {
                IssueNumber = new TextSource()
                {
                    InitialLabel = "Issue Number",
                    Name = "IssueNumber",
                    Source = new FieldSourceDTO
                    {
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields,
                        RequestUpstream = true
                    },
                    Events = new List<ControlEvent>()
                    {
                        ControlEvent.RequestConfig
                    }
                };

                Controls.Add(IssueNumber);
            }
        }

        private const string RunTimeCrateLabel = "Jira Issue Details";

        private readonly AtlassianService _atlassianService;

        public Get_Jira_Issue_v1(ICrateManager crateManager, AtlassianService atlassianService) 
            : base(crateManager)
        {
            _atlassianService = atlassianService;
        }

        public override async Task Initialize()
        {
            CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(RunTimeCrateLabel);
            await Task.Yield();
        }

        public override async Task FollowUp()
        {
            var issueKey = ActivityUI.IssueNumber.GetValue(Storage);
            if (!string.IsNullOrEmpty(issueKey))
            {
                var issueFields = _atlassianService.GetJiraIssue(issueKey, AuthorizationToken);
                CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(RunTimeCrateLabel).AddFields(issueFields);
            }
            await Task.Yield();
        }

        public override async Task Run()
        {
            var issueKey = ActivityUI.IssueNumber.GetValue(Storage);
            if (!string.IsNullOrEmpty(issueKey))
            {
                var issueFields = _atlassianService.GetJiraIssue(issueKey, AuthorizationToken);
                Payload.Add(CrateJiraIssueDetailsPayloadCrate(issueFields));
            }

            await Task.Yield();
        }

        private Crate CrateJiraIssueDetailsPayloadCrate(List<FieldDTO> curJiraIssue)
        {
            return Crate.FromContent(RunTimeCrateLabel, new StandardPayloadDataCM(curJiraIssue), AvailabilityType.RunTime);
        }
    }
}