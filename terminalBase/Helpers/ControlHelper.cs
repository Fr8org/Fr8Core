using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using TerminalBase.Models;

namespace TerminalBase.Helpers
{
    public class ControlHelper
    {
        private readonly IHubCommunicator _hubCommunicator;
        private readonly ActivityContext _activityContext;
        public ControlHelper(ActivityContext activityContext, IHubCommunicator hubCommunicator)
        {
            _hubCommunicator = hubCommunicator;
            _activityContext = activityContext;
        }

        public StandardConfigurationControlsCM GetConfigurationControls(ICrateStorage storage)
        {
            return storage.CrateContentsOfType<StandardConfigurationControlsCM>(c => c.Label == BaseTerminalActivity.ConfigurationControlsLabel).FirstOrDefault();
        }

        public T GetControl<T>(StandardConfigurationControlsCM configurationControls,string name, string controlType = null) where T : ControlDefinitionDTO
        {
            Func<ControlDefinitionDTO, bool> predicate = x => x.Name == name;
            if (controlType != null)
            {
                predicate = x => x.Type == controlType && x.Name == name;
            }

            return (T)configurationControls.Controls.FirstOrDefault(predicate);
        }

        /// <summary>
        /// This is a generic function for creating a CrateChooser which is suitable for most use-cases
        /// </summary>
        /// <param name="curTerminalActivity"></param>
        /// <param name="label"></param>
        /// <param name="name"></param>
        /// <param name="singleManifest"></param>
        /// <param name="requestUpstream"></param>
        /// <param name="requestConfig"></param>
        /// <returns></returns>
        public async Task<CrateChooser> GenerateCrateChooser(
            string name,
            string label,
            bool singleManifest,
            bool requestUpstream = false,
            bool requestConfig = false)
        {
            var crateDescriptions = await _hubCommunicator.GetCratesByDirection<CrateDescriptionCM>(_activityContext.ActivityPayload.Id, CrateDirection.Upstream, _activityContext.UserId);
            var runTimeCrateDescriptions = crateDescriptions.Where(c => c.Availability == AvailabilityType.RunTime || c.Availability == AvailabilityType.Always).SelectMany(c => c.Content.CrateDescriptions);
            var control = new CrateChooser
            {
                Label = label,
                Name = name,
                CrateDescriptions = runTimeCrateDescriptions.ToList(),
                SingleManifestOnly = singleManifest,
                RequestUpstream = requestUpstream
            };

            if (requestConfig)
            {
                control.Events.Add(new ControlEvent("onChange", "requestConfig"));
            }

            return control;
        }

        /// <summary>
        /// Creates TextBlock control and fills it with label, value and CssClass
        /// </summary>
        /// <param name="curLabel">Label</param>
        /// <param name="curValue">Value</param>
        /// <param name="curCssClass">Css Class</param>
        /// <param name="curName">Name</param>
        /// <returns>TextBlock control</returns>
        public TextBlock GenerateTextBlock(string curLabel, string curValue, string curCssClass, string curName = "unnamed")
        {
            return new TextBlock
            {
                Name = curName,
                Label = curLabel,
                Value = curValue,
                CssClass = curCssClass
            };
        }

        public UpstreamCrateChooser CreateUpstreamCrateChooser(string name, string label, bool isMultiSelection = true)
        {

            var manifestDdlb = new DropDownList { Name = name + "_mnfst_dropdown_0", Source = new FieldSourceDTO(CrateManifestTypes.StandardDesignTimeFields, "AvailableUpstreamManifests") };
            var labelDdlb = new DropDownList { Name = name + "_lbl_dropdown_0", Source = new FieldSourceDTO(CrateManifestTypes.StandardDesignTimeFields, "AvailableUpstreamLabels") };

            var ctrl = new UpstreamCrateChooser
            {
                Name = name,
                Label = label,
                SelectedCrates = new List<CrateDetails> { new CrateDetails { Label = labelDdlb, ManifestType = manifestDdlb } },
                MultiSelection = isMultiSelection
            };

            return ctrl;
        }

        /// <summary>
        /// Creates RadioButtonGroup to enter specific value or choose value from upstream crate.
        /// </summary>
        public TextSource CreateSpecificOrUpstreamValueChooser(
            string label,
            string controlName,
            string upstreamSourceLabel = "",
            string filterByTag = "",
            bool addRequestConfigEvent = false,
            bool requestUpstream = false)
        {
            var control = new TextSource(label, upstreamSourceLabel, controlName)
            {
                Source = new FieldSourceDTO
                {
                    Label = upstreamSourceLabel,
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields,
                    FilterByTag = filterByTag,
                    RequestUpstream = requestUpstream
                }
            };
            if (addRequestConfigEvent)
            {
                control.Events.Add(new ControlEvent("onChange", "requestConfig"));
            }

            return control;
        }

        /// <summary>
        /// specify selected FieldDO for DropDownList
        /// specify Value (TimeSpan) for Duration
        /// </summary>
        public void SetControlValue(ActivityPayload activity, string controlFullName, object value)
        {
            var crateStorage = activity.CrateStorage;
            var controls = GetConfigurationControls(crateStorage);

            if (controls != null)
            {
                var control = TraverseNestedControls(controls.Controls, controlFullName);

                if (control != null)
                {
                    switch (control.Type)
                    {
                        case "TextBlock":
                        case "TextBox":
                        case "BuildMessageAppender":
                        case ControlTypes.TextArea:
                            control.Value = (string)value;
                            break;

                        case "CheckBox":
                            control.Selected = true;
                            break;

                        case "DropDownList":
                            var ddlb = control as DropDownList;
                            var val = value as ListItem;
                            ddlb.selectedKey = val.Key;
                            ddlb.Value = val.Value;
                            //ddlb.ListItems are not loaded yet
                            break;

                        case "Duration":
                            var duration = control as Duration;
                            var timespan = (TimeSpan)value;
                            duration.Days = timespan.Days;
                            duration.Hours = timespan.Hours;
                            duration.Minutes = timespan.Minutes;
                            break;

                        default:
                            throw new NotSupportedException($"Unsupported control type {control.Type}");
                    }
                }
            }
        }

        private ControlDefinitionDTO TraverseNestedControls(List<ControlDefinitionDTO> controls, string childControl)
        {
            ControlDefinitionDTO controlDefinitionDTO = null;
            var controlNames = childControl.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (controlNames.Length > 0 && controls.Count > 0)
            {
                var control = controls.FirstOrDefault(a => a.Name == controlNames[0]);

                if (control != null)
                {
                    if (controlNames.Count() == 1)
                    {
                        controlDefinitionDTO = control;
                    }
                    else
                    {
                        List<ControlDefinitionDTO> nestedControls = null;

                        if (control.Type == "RadioButtonGroup")
                        {
                            var radio = (control as RadioButtonGroup).Radios.FirstOrDefault(a => a.Name == controlNames[1]);
                            if (radio != null)
                            {
                                radio.Selected = true;
                                nestedControls = radio.Controls.ToList();

                                controlDefinitionDTO = TraverseNestedControls(nestedControls, string.Join(".", controlNames.Skip(2)));
                            }
                        }
                        //TODO: Add support for future controls with nested child controls
                        else
                            throw new NotImplementedException("Can't search for controls inside of " + control.Type);
                    }
                }
            }

            return controlDefinitionDTO;
        }

        public void RemoveControl<T>(StandardConfigurationControlsCM configurationControls, string name) where T : ControlDefinitionDTO
        {
            var control = GetControl<T>(configurationControls, name);
            if (control != null)
            {
                configurationControls.Controls.Remove(control);
            }
        }

        public string ParseConditionToText(List<FilterConditionDTO> filterData)
        {
            var parsedConditions = new List<string>();

            filterData.ForEach(condition =>
            {
                string parsedCondition = condition.Field;

                switch (condition.Operator)
                {
                    case "eq":
                        parsedCondition += " = ";
                        break;
                    case "neq":
                        parsedCondition += " != ";
                        break;
                    case "gt":
                        parsedCondition += " > ";
                        break;
                    case "gte":
                        parsedCondition += " >= ";
                        break;
                    case "lt":
                        parsedCondition += " < ";
                        break;
                    case "lte":
                        parsedCondition += " <= ";
                        break;
                    default:
                        throw new NotSupportedException(string.Format("Not supported operator: {0}", condition.Operator));
                }

                parsedCondition += string.Format("'{0}'", condition.Value);
                parsedConditions.Add(parsedCondition);
            });

            var finalCondition = string.Join(" AND ", parsedConditions);

            return finalCondition;
        }
    }
}
