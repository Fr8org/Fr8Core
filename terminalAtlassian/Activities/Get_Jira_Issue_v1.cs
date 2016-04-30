using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using StructureMap;
using terminalAtlassian.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalAtlassian.Actions
{
    public class Get_Jira_Issue_v1 : EnhancedTerminalActivity<Get_Jira_Issue_v1.ActivityUi>
    {
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

        public Get_Jira_Issue_v1() : base(true)
        {
            _atlassianService = ObjectFactory.GetInstance<AtlassianService>();
        }

        protected override async Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
            await Task.Yield();
        }

        protected override async Task Configure(RuntimeCrateManager runtimeCrateManager)
        {
            var issueKey = ConfigurationControls.IssueNumber.GetValue(CurrentActivityStorage);
            if (!string.IsNullOrEmpty(issueKey))
            {
                var issueFields = _atlassianService.GetJiraIssue(issueKey, AuthorizationToken);
                CurrentActivityStorage.ReplaceByLabel(CrateJiraIssueDetailsDescriptionCrate(issueFields));
            }

            await Task.Yield();
        }

        protected override async Task RunCurrentActivity()
        {
            var issueKey = ConfigurationControls.IssueNumber.GetValue(CurrentActivityStorage);
            if (!string.IsNullOrEmpty(issueKey))
            {
                var issueFields = _atlassianService.GetJiraIssue(issueKey, AuthorizationToken);
                CurrentPayloadStorage.Add(CrateJiraIssueDetailsPayloadCrate(issueFields));
            }

            await Task.Yield();
        }

        private Crate CrateJiraIssueDetailsDescriptionCrate(List<FieldDTO> curJiraIssue)
        {
            return Data.Crates.Crate.FromContent("Jira Issue Details", new FieldDescriptionsCM(curJiraIssue), Data.States.AvailabilityType.Configuration);
        }

        private Crate CrateJiraIssueDetailsPayloadCrate(List<FieldDTO> curJiraIssue)
        {
            return Data.Crates.Crate.FromContent("Jira Issue Details", new StandardPayloadDataCM(curJiraIssue), Data.States.AvailabilityType.RunTime);
        }
    }
}