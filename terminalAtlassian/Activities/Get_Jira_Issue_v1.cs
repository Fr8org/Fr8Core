using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using StructureMap;
using terminalAtlassian.Services;
using TerminalBase.BaseClasses;

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
            return Fr8Data.Crates.Crate.FromContent("Jira Issue Details", new FieldDescriptionsCM(curJiraIssue), Fr8Data.States.AvailabilityType.Configuration);
        }

        private Crate CrateJiraIssueDetailsPayloadCrate(List<FieldDTO> curJiraIssue)
        {
            return Fr8Data.Crates.Crate.FromContent("Jira Issue Details", new StandardPayloadDataCM(curJiraIssue), Fr8Data.States.AvailabilityType.RunTime);
        }
    }
}