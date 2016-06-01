using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TerminalBase.Infrastructure;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Fr8Data.States;
using TerminalBase.Services;

namespace TerminalBase.BaseClasses
{
    public abstract partial class BaseTerminalActivity : BaseTerminalActivityLegacy
    {
        private StandardConfigurationControlsCM _configurationControls;
        protected StandardConfigurationControlsCM ConfigurationControls => _configurationControls ?? (_configurationControls = GetConfigurationControls());

        protected BaseTerminalActivity(bool isAuthenticationRequired, ICrateManager crateManager)
          : base(crateManager)
        {
        }

        protected override void InitializeInternalState()
        {
            CrateSignaller = new CrateSignaller(Storage, MyTemplate.Name, ActivityId);
        }

        protected string ExtractPayloadFieldValue(string fieldKey)
        {
            var fieldValues = Payload.CratesOfType<StandardPayloadDataCM>().SelectMany(x => x.Content.GetValues(fieldKey))
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();
            if (fieldValues.Length > 0)
                return fieldValues[0];
            var baseEvent = new BaseTerminalEvent();
            var exceptionMessage = $"No field found with specified key: {fieldKey}.";
            //This is required for proper logging of the Incidents
            baseEvent.SendTerminalErrorIncident(MyTemplate.Terminal.Name, exceptionMessage, MyTemplate.Name, CurrentUserId);

            SendEventReport(exceptionMessage);

            throw new ApplicationException(exceptionMessage);
        }
        
        protected StandardConfigurationControlsCM GetConfigurationControls()
        {
            return ControlHelper.GetConfigurationControls(Storage);
        }

        protected T GetControl<T>(string name) where T : ControlDefinitionDTO
        {
            return ConfigurationControls?.FindByNameNested<T>(name);
        }

        protected void RemoveControl<T>(string name) where T : ControlDefinitionDTO
        {
            ControlHelper.RemoveControl<T>(ConfigurationControls, name);
        }

        protected void AddLabelControl(string name, string label, string text)
        {
            AddControl(ControlHelper.GenerateTextBlock(label, text, "well well-lg", name));
        }

        protected void AddControl(ControlDefinitionDTO control)
        {
            EnsureControlsCrate();
            ConfigurationControls.Controls.Add(control);
        }

        public virtual IEnumerable<FieldDTO> GetRequiredFields(string crateLabel)
        {
            var requiredFields = Storage
                .CrateContentsOfType<FieldDescriptionsCM>(c => c.Label.Equals(crateLabel))
                .SelectMany(f => f.Fields.Where(s => s.IsRequired));
            return requiredFields;
        }

        protected void UpdateDesignTimeCrateValue(string label, params FieldDTO[] fields)
        {
            var crate = Storage.CratesOfType<FieldDescriptionsCM>().FirstOrDefault(x => x.Label == label);

            if (crate == null)
            {
                crate = CrateManager.CreateDesignTimeFieldsCrate(label, fields);
                Storage.Add(crate);
            }
            else
            {
                crate.Content.Fields.Clear();
                crate.Content.Fields.AddRange(fields);
            }
        }

        protected Crate<StandardConfigurationControlsCM> PackControlsCrate(params ControlDefinitionDTO[] controlsList)
        {
            return Crate<StandardConfigurationControlsCM>.FromContent(ConfigurationControlsLabel, new StandardConfigurationControlsCM(controlsList), AvailabilityType.Configuration);
        }

        protected Crate PackControls(StandardConfigurationControlsCM page)
        {
            return PackControlsCrate(page.Controls.ToArray());
        }

        protected Crate<StandardConfigurationControlsCM> EnsureControlsCrate()
        {
            var controlsCrate = Storage.CratesOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (controlsCrate == null)
            {
                controlsCrate = CrateManager.CreateStandardConfigurationControlsCrate(ConfigurationControlsLabel);
                Storage.Add(controlsCrate);
            }

            _configurationControls = controlsCrate.Content;

            return controlsCrate;
        }
    }
}