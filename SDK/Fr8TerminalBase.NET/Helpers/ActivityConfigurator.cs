using System;
using System.Linq;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Models;

namespace Fr8.TerminalBase.Helpers
{
    public class ActivityConfigurator
    {
        private readonly ActivityPayload _activity;
        private StandardConfigurationControlsCM _configurationControls;

        public StandardConfigurationControlsCM ConfigurationControls
        {
            get
            {
                if (_configurationControls == null)
                {
                    _configurationControls = _activity.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>(c => c.Label == TerminalActivityBase.ConfigurationControlsLabel).FirstOrDefault();
                }

                return _configurationControls;
            }
        }

        public ActivityPayload Activity => _activity;

        public ActivityConfigurator(ActivityPayload activity)
        {
            _activity = activity;
        }

        public T GetControl<T>(string name, string controlType = null)
            where T : ControlDefinitionDTO
        {
            Func<ControlDefinitionDTO, bool> predicate = x => x.Name == name;
            if (controlType != null)
            {
                predicate = x => x.Type == controlType && x.Name == name;
            }

            return (T)ConfigurationControls?.Controls?.FirstOrDefault(predicate);
        }

        /// <summary>
        /// specify selected FieldDO for DropDownList
        /// specify Value (TimeSpan) for Duration
        /// </summary>
        public void SetControlValue(string controlFullName, object value)
        {

            var result = ConfigurationControls?.FindByNameNested(controlFullName);

            var control = result as ControlDefinitionDTO;

            if (control != null)
                switch (control.Type)
                {
                    case "TextBlock":
                    case "TextBox":
                    case "BuildMessageAppender":
                    case ControlTypes.TextArea:
                        control.Value = (string)value;
                        break;

                    case "CheckBox":
                        control.Selected = Convert.ToBoolean(value);
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
            else if (result is ISelectable)
            {
                (result as ISelectable).Selected = Convert.ToBoolean(value);
            }
            else
                throw new ApplicationException();


        }

        public static void SetControlValue(ActivityPayload activity, string controlFullName, object value)
        {
            new ActivityConfigurator(activity).SetControlValue(controlFullName, value);
        }

        public static StandardConfigurationControlsCM GetConfigurationControls(ActivityPayload activity)
        {
            return new ActivityConfigurator(activity).ConfigurationControls;
        }

        public static T GetControl<T>(ActivityPayload activity, string name, string controlType = null)
              where T : ControlDefinitionDTO
        {
            return new ActivityConfigurator(activity).GetControl<T>(name, controlType);
        }
    }
}
