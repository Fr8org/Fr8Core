using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using fr8.Infrastructure.Data.Control;
using fr8.Infrastructure.Data.Crates;
using fr8.Infrastructure.Data.DataTransferObjects;
using fr8.Infrastructure.Data.Managers;
using fr8.Infrastructure.Data.Manifests;
using fr8.Infrastructure.Data.States;
using StructureMap;
using terminalAtlassian.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalAtlassian.Actions
{
    public class Get_Jira_Issue_v1 : EnhancedTerminalActivity<Get_Jira_Issue_v1.ActivityUi>
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


        private readonly AtlassianService _atlassianService;

        public Get_Jira_Issue_v1(ICrateManager crateManager, AtlassianService atlassianService) 
            : base(crateManager)
        {
            _atlassianService = atlassianService;
        }

        public override async Task Initialize()
        {
            await Task.Yield();
        }

        public override async Task FollowUp()
        {
            var issueKey = ActivityUI.IssueNumber.GetValue(Storage);
            if (!string.IsNullOrEmpty(issueKey))
            {
                var issueFields = _atlassianService.GetJiraIssue(issueKey, AuthorizationToken);
                Storage.ReplaceByLabel(CrateJiraIssueDetailsDescriptionCrate(issueFields));
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

        private Crate CrateJiraIssueDetailsDescriptionCrate(List<FieldDTO> curJiraIssue)
        {
            return Crate.FromContent("Jira Issue Details", new FieldDescriptionsCM(curJiraIssue), AvailabilityType.RunTime);
        }

        private Crate CrateJiraIssueDetailsPayloadCrate(List<FieldDTO> curJiraIssue)
        {
            return Crate.FromContent("Jira Issue Details", new StandardPayloadDataCM(curJiraIssue), AvailabilityType.RunTime);
        }
    }
}