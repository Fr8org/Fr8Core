using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StructureMap;
using Data.Control;
using Data.Infrastructure;
using Data.Interfaces.Manifests;
using TerminalBase.BaseClasses;
using terminalAtlassian.Services;

namespace terminalAtlassian.Actions
{
    public class Save_Jira_Issue_v1 : EnhancedTerminalActivity<Save_Jira_Issue_v1.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList AvailableProjects { get; set; }

            public TextBlock SelectProjectLabel { get; set; }

            public DropDownList AvailableIssueTypes { get; set; }

            public TextBlock SelectIssueTypeLabel { get; set; }

            public ActivityUi()
            {
                AvailableProjects = new DropDownList()
                {
                    Label = "Projects",
                    Name = "AvailableProjects",
                    Value = null,
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                    Source = null
                };
                Controls.Add(AvailableProjects);

                SelectProjectLabel = new TextBlock()
                {
                    Value = "Please select Jira project.",
                    Name = "SelectProjectLabel",
                    CssClass = "well well-lg",
                    IsHidden = false
                };
                Controls.Add(SelectProjectLabel);

                AvailableIssueTypes = new DropDownList()
                {
                    Label = "Issue Types",
                    Name = "AvailableIssueTypes",
                    Value = null,
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                    Source = null,
                    IsHidden = true
                };
                Controls.Add(AvailableIssueTypes);

                SelectIssueTypeLabel = new TextBlock()
                {
                    Value = "Please select Jira issue type.",
                    Name = "SelectIssueTypeLabel",
                    CssClass = "well well-lg",
                    IsHidden = true
                };
                Controls.Add(SelectIssueTypeLabel);
            }
        }


        private readonly AtlassianService _atlassianService;

        public Save_Jira_Issue_v1() : base(true)
        {
            _atlassianService = ObjectFactory.GetInstance<AtlassianService>();
        }

        #region Configuration

        protected override async Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
            ConfigurationControls.AvailableProjects.ListItems = _atlassianService
                .GetProjects(AuthorizationToken)
                .ToListItems()
                .ToList();
        }

        protected override async Task Configure(RuntimeCrateManager runtimeCrateManager)
        {
            var projectKey = ConfigurationControls.AvailableProjects.Value;
            if (!string.IsNullOrEmpty(projectKey))
            {
                FillIssueTypeDdl(projectKey);
                ToggleProjectSelectedVisibility(true);
            }
            else
            {
                ToggleProjectSelectedVisibility(false);
            }
        }

        private void ToggleProjectSelectedVisibility(bool projectSelected)
        {
            ConfigurationControls.SelectProjectLabel.IsHidden = projectSelected;
            ConfigurationControls.AvailableIssueTypes.IsHidden = !projectSelected;
            ConfigurationControls.SelectIssueTypeLabel.IsHidden = !projectSelected;
        }

        private void FillIssueTypeDdl(string projectKey)
        {
            ConfigurationControls.AvailableIssueTypes.ListItems = _atlassianService
                .GetIssueTypes(projectKey, AuthorizationToken)
                .ToListItems()
                .ToList();
        }

        #endregion Configuration


        #region Runtime

        protected override async Task RunCurrentActivity()
        {
            await Task.Yield();
        }

        #endregion Runtime
    }
}