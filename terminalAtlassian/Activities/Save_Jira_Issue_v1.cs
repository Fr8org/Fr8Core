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
using Newtonsoft.Json;
using StructureMap;
using terminalAtlassian.Interfaces;
using terminalAtlassian.Services;

namespace terminalAtlassian.Actions
{
    public class Save_Jira_Issue_v1 : EnhancedTerminalActivity<Save_Jira_Issue_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Version = "1",
            Name = "Save_Jira_Issue",
            Label = "Save Jira Issue",
            NeedsAuthentication = true,
            Category = ActivityCategory.Forwarders,
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList AvailableProjects { get; set; }

            public TextBlock SelectProjectLabel { get; set; }

            public DropDownList AvailableIssueTypes { get; set; }

            public TextBlock SelectIssueTypeLabel { get; set; }

            public TextSource Summary { get; set; }

            public TextSource Description { get; set; }

            public DropDownList AvailablePriorities { get; set; }

            public DropDownList AssigneeSelector { get; set; }
            public ControlDefinitionDTO SprintFieldName { get; set; }


            public DropDownList Sprint { get; set; }

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

                Sprint = new DropDownList()
                {
                    Name = "Sprint",
                    Label = "Sprint",
                    IsHidden = true
                };
                Controls.Add(Sprint);

                AvailablePriorities = new DropDownList()
                {
                    Label = "Priority",
                    Name = "AvailablePriorities",
                    Value = null,
                    Source = null,
                    IsHidden = true
                };
                Controls.Add(AvailablePriorities);

                AssigneeSelector = new DropDownList
                {
                    Label = "Asignee",
                    Name = "Asignee",
                    IsHidden = true
                };
                Controls.Add(AssigneeSelector);

                Summary = new TextSource()
                {
                    Label = "Summary",
                    Name = "SummaryTextSource",
                    InitialLabel = "Summary",
                    IsHidden = true
                };
                Controls.Add(Summary);

                Description = new TextSource()
                {
                    Label = "Description",
                    Name = "DescriptionTextSource",
                    InitialLabel = "Description",
                    IsHidden = true
                };
                Controls.Add(Description);

                SprintFieldName = new ControlDefinitionDTO()
                {
                    Name = "SprintFieldName",
                    IsHidden = true
                };
                Controls.Add(SprintFieldName);
            }

            public void AppendCustomFields(IEnumerable<FieldDTO> customFields)
            {
                var toBeRemoved = new List<ControlDefinitionDTO>();
                foreach (var control in Controls)
                {
                    if (control.Name.StartsWith("CustomField_"))
                    {
                        toBeRemoved.Add(control);
                    }
                }

                foreach (var control in toBeRemoved)
                {
                    Controls.Remove(control);
                }

                foreach (var customField in customFields)
                {
                    if (customField.Key != "Sprint")
                    {
                        Controls.Add(new TextSource()
                        {
                            Name = "CustomField_" + customField.Value,
                            InitialLabel = customField.Key,
                            Source = new FieldSourceDTO()
                            {
                                ManifestType = CrateManifestTypes.StandardDesignTimeFields,
                                RequestUpstream = true,
                                AvailabilityType = AvailabilityType.RunTime
                            }
                        });
                    }
                    else
                    {
                        SprintFieldName.Label = customField.Value;
                    }
                }
            }

            public void RestoreCustomFields(ICrateStorage crateStorage)
            {
                var controls = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().First();

                foreach (var control in controls.Controls)
                {
                    if (control.Name.StartsWith("CustomField_"))
                    {
                        Controls.Add(control);
                    }
                }
            }

            public IEnumerable<FieldDTO> GetValues(ICrateStorage crateStorage)
            {
                var result = new List<FieldDTO>();

                foreach (var control in Controls)
                {
                    if (control.Name.StartsWith("CustomField_"))
                    {
                        var textSource = (TextSource)control;

                        var key = control.Name.Substring("CustomField_".Length);
                        var value = textSource.GetValue(crateStorage);

                        if (!string.IsNullOrEmpty(value))
                        {
                            result.Add(new FieldDTO(key, value));
                        }
                    }
                }

                return result;
            }
        }

        public class ConfigurationProperties
        {
            public string SelectedProjectKey { get; set; }

            public string SelectedIssueType { get; set; }
        }


        private const string ConfigurationPropertiesLabel = "ConfigurationProperties";
        private readonly IAtlassianService _atlassianService;

        public Save_Jira_Issue_v1(ICrateManager crateManager, IAtlassianService atlassianService)
            : base(crateManager)
        {
            _atlassianService = atlassianService;
        }

        #region Configuration

        public override async Task Initialize()
        {
            ActivityUI.AvailableProjects.ListItems = _atlassianService
                .GetProjects(AuthorizationToken)
                .ToListItems()
                .ToList();

            await Task.Yield();
        }

        public override async Task FollowUp()
        {
            ActivityUI.RestoreCustomFields(Storage);
            var configProps = GetConfigurationProperties();

            var projectKey = ActivityUI.AvailableProjects.Value;
            if (!string.IsNullOrEmpty(projectKey))
            {
                ToggleProjectSelectedVisibility(true);

                if (projectKey != configProps.SelectedProjectKey)
                {
                    FillIssueTypeDdl(projectKey);
                    await FillAssigneeSelector(projectKey);
                }

                var issueTypeKey = ActivityUI.AvailableIssueTypes.Value;
                if (!string.IsNullOrEmpty(issueTypeKey))
                {
                    ToggleFieldsVisibility(true);

                    if (configProps.SelectedIssueType != issueTypeKey)
                    {
                        await FillFieldDdls();
                    }
                }
                else
                {
                    ToggleFieldsVisibility(false);
                }
            }
            else
            {
                ToggleProjectSelectedVisibility(false);
            }

            configProps.SelectedProjectKey = ActivityUI.AvailableProjects.Value;
            configProps.SelectedIssueType = ActivityUI.AvailableIssueTypes.Value;

            SetConfigurationProperties(configProps);

            await Task.Yield();
        }

        private async Task FillAssigneeSelector(string projectKey)
        {
            var users = await _atlassianService.GetUsersAsync(projectKey, AuthorizationToken);
            ActivityUI.AssigneeSelector.ListItems = users.Select(x => new ListItem { Key = x.DisplayName, Value = x.Key }).ToList();
        }

        private ConfigurationProperties GetConfigurationProperties()
        {
            var fd = Storage
                .CrateContentsOfType<FieldDescriptionsCM>(x => x.Label == ConfigurationPropertiesLabel)
                .FirstOrDefault();

            var result = new ConfigurationProperties();
            if (fd != null)
            {
                result.SelectedProjectKey = fd.Fields.FirstOrDefault(x => x.Key == "SelectedProjectKey")?.Value;
                result.SelectedIssueType = fd.Fields.FirstOrDefault(x => x.Key == "SelectedIssueType")?.Value;
            }

            return result;
        }

        private void SetConfigurationProperties(ConfigurationProperties configProps)
        {
            var fd = new FieldDescriptionsCM(
                new FieldDTO("SelectedProjectKey", configProps.SelectedProjectKey, AvailabilityType.Configuration),
                new FieldDTO("SelectedIssueType", configProps.SelectedIssueType, AvailabilityType.Configuration)
            );

            Storage.ReplaceByLabel(Crate.FromContent(ConfigurationPropertiesLabel, fd, AvailabilityType.Configuration));
        }

        private void ToggleProjectSelectedVisibility(bool projectSelected)
        {
            ActivityUI.SelectProjectLabel.IsHidden = projectSelected;
            ActivityUI.AvailableIssueTypes.IsHidden = !projectSelected;
            ActivityUI.SelectIssueTypeLabel.IsHidden = !projectSelected;
        }

        private void FillIssueTypeDdl(string projectKey)
        {
            ActivityUI.AvailableIssueTypes.ListItems = _atlassianService
                .GetIssueTypes(projectKey, AuthorizationToken)
                .ToListItems()
                .ToList();
        }

        private void ToggleFieldsVisibility(bool visible)
        {
            ActivityUI.Summary.IsHidden = !visible;
            ActivityUI.Description.IsHidden = !visible;
            ActivityUI.AvailablePriorities.IsHidden = !visible;
            ActivityUI.Sprint.IsHidden = !visible;
            ActivityUI.SelectIssueTypeLabel.IsHidden = visible;
            ActivityUI.AssigneeSelector.IsHidden = !visible;
        }

        private async Task FillFieldDdls()
        {
            ActivityUI.AvailablePriorities.ListItems = _atlassianService
                .GetPriorities(AuthorizationToken)
                .ToListItems()
                .ToList();

            ActivityUI.Sprint.ListItems = await _atlassianService.GetSprints(AuthorizationToken, ActivityUI.AvailableProjects.Value);
            var customFields = _atlassianService.GetCustomFields(AuthorizationToken);
            ActivityUI.AppendCustomFields(customFields);
        }

        #endregion Configuration


        #region Runtime

        public override async Task Run()
        {
            ActivityUI.RestoreCustomFields(Storage);

            var issueInfo = ExtractIssueInfo();
            await _atlassianService.CreateIssue(issueInfo, AuthorizationToken);

            var credentialsDTO = JsonConvert.DeserializeObject<CredentialsDTO>(AuthorizationToken.Token);
            await
                PushUserNotification("Success", "Jira issue created",
                    "Created new jira issue: " + credentialsDTO.Domain + "/browse/" + issueInfo.Key);
        }

        private IssueInfo ExtractIssueInfo()
        {
            var projectKey = ActivityUI.AvailableProjects.Value;
            if (string.IsNullOrEmpty(projectKey))
            {
                throw new ApplicationException("Jira Project is not selected.");
            }

            var issueTypeKey = ActivityUI.AvailableIssueTypes.Value;
            if (string.IsNullOrEmpty(issueTypeKey))
            {
                throw new ApplicationException("Jira Issue Type is not selected.");
            }

            var result = new IssueInfo()
            {
                ProjectKey = projectKey,
                IssueTypeKey = issueTypeKey,
                PriorityKey = ActivityUI.AvailablePriorities.Value,
                Description = ActivityUI.Description.GetValue(Payload),
                Summary = ActivityUI.Summary.GetValue(Payload),
                CustomFields = ActivityUI.GetValues(Payload).ToList(),
                Assignee = ActivityUI.AssigneeSelector.Value
            };


            if (ActivityUI.Sprint.Value != null)
            {
                result.CustomFields.Add(new FieldDTO() { Key = ActivityUI.SprintFieldName.Label, Value = ActivityUI.Sprint.Value });
            }

            return result;
        }

        #endregion Runtime
    }
}
