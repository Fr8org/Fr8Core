using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StructureMap;
using Data.Control;
using Data.Crates;
using Data.Infrastructure;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure.Behaviors;
using terminalAtlassian.Interfaces;
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

            public TextSource Summary { get; set; }

            public TextSource Description { get; set; }

            public DropDownList AvailablePriorities { get; set; }

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

                AvailablePriorities = new DropDownList()
                {
                    Label = "Priority",
                    Name = "AvailablePriorities",
                    Value = null,
                    Source = null,
                    IsHidden = true
                };
                Controls.Add(AvailablePriorities);

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

            await Task.Yield();
        }

        protected override async Task Configure(RuntimeCrateManager runtimeCrateManager)
        {
            ConfigurationControls.RestoreCustomFields(CurrentActivityStorage);
            var configProps = GetConfigurationProperties();
            
            var projectKey = ConfigurationControls.AvailableProjects.Value;
            if (!string.IsNullOrEmpty(projectKey))
            {
                ToggleProjectSelectedVisibility(true);

                if (projectKey != configProps.SelectedProjectKey)
                {
                    FillIssueTypeDdl(projectKey);
                }

                var issueTypeKey = ConfigurationControls.AvailableIssueTypes.Value;
                if (!string.IsNullOrEmpty(issueTypeKey))
                {
                    ToggleFieldsVisibility(true);

                    if (configProps.SelectedIssueType != issueTypeKey)
                    {
                        FillFieldDdls();
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

            configProps.SelectedProjectKey = ConfigurationControls.AvailableProjects.Value;
            configProps.SelectedIssueType = ConfigurationControls.AvailableIssueTypes.Value;

            SetConfigurationProperties(configProps);

            await Task.Yield();
        }

        private ConfigurationProperties GetConfigurationProperties()
        {
            var fd = CurrentActivityStorage
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

            CurrentActivityStorage.ReplaceByLabel(Crate.FromContent(ConfigurationPropertiesLabel, fd, AvailabilityType.Configuration));
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

        private void ToggleFieldsVisibility(bool visible)
        {
            ConfigurationControls.Summary.IsHidden = !visible;
            ConfigurationControls.Description.IsHidden = !visible;
            ConfigurationControls.AvailablePriorities.IsHidden = !visible;
            ConfigurationControls.SelectIssueTypeLabel.IsHidden = visible;
        }

        private void FillFieldDdls()
        {
            ConfigurationControls.AvailablePriorities.ListItems = _atlassianService
                .GetPriorities(AuthorizationToken)
                .ToListItems()
                .ToList();

            var customFields = _atlassianService.GetCustomFields(AuthorizationToken);
            ConfigurationControls.AppendCustomFields(customFields);
        }

        #endregion Configuration


        #region Runtime

        protected override async Task RunCurrentActivity()
        {
            ConfigurationControls.RestoreCustomFields(CurrentActivityStorage);

            var issueInfo = ExtractIssueInfo();
            _atlassianService.CreateIssue(issueInfo, AuthorizationToken);

            var credentialsDTO = JsonConvert.DeserializeObject<CredentialsDTO>(AuthorizationToken.Token);

            await PushUserNotification(new TerminalNotificationDTO
            {
                Type = "Success",
                ActivityName = "Save_Jira_Issue",
                ActivityVersion = "1",
                TerminalName = "terminalAtlassian",
                TerminalVersion = "1",
                Message = "Created new jira issue: " + credentialsDTO.Domain + "/browse/" + issueInfo.Key,
                Subject = "Jira issue created"
            });
        }

        private IssueInfo ExtractIssueInfo()
        {
            var projectKey = ConfigurationControls.AvailableProjects.Value;
            if (string.IsNullOrEmpty(projectKey))
            {
                throw new ApplicationException("Jira Project is not selected.");
            }

            var issueTypeKey = ConfigurationControls.AvailableIssueTypes.Value;
            if (string.IsNullOrEmpty(issueTypeKey))
            {
                throw new ApplicationException("Jira Issue Type is not selected.");
            }

            var result = new IssueInfo()
            {
                ProjectKey = projectKey,
                IssueTypeKey = issueTypeKey,
                PriorityKey = ConfigurationControls.AvailablePriorities.Value,
                Description = ConfigurationControls.Description.GetValue(CurrentPayloadStorage),
                Summary = ConfigurationControls.Summary.GetValue(CurrentPayloadStorage),
                CustomFields = ConfigurationControls.GetValues(CurrentPayloadStorage).ToList()
            };

            return result;
        }

        #endregion Runtime
    }
}