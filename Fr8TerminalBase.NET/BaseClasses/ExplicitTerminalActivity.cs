using System.Collections.Generic;
using System.Linq;
using System.Web.Compilation;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;

namespace Fr8.TerminalBase.BaseClasses
{
    public abstract class ExplicitTerminalActivity : TerminalActivityBase
    {
        private StandardConfigurationControlsCM _configurationControls;

        protected StandardConfigurationControlsCM ConfigurationControls => _configurationControls ?? (_configurationControls = GetConfigurationControls());

        protected ExplicitTerminalActivity(ICrateManager crateManager)
            : base(crateManager)
        {
        }

        private StandardConfigurationControlsCM GetConfigurationControls()
        {
            if (!IsRuntime)
            {
                EnsureControlsCrate();
            }

            return Storage.CrateContentsOfType<StandardConfigurationControlsCM>(c => c.Label == ConfigurationControlsLabel).FirstOrDefault();
        }

        protected T GetControl<T>(string name) where T : ControlDefinitionDTO
        {
            return ConfigurationControls?.FindByNameNested<T>(name);
        }

        protected void RemoveControl<T>(string name) where T : ControlDefinitionDTO
        {
            var control = ConfigurationControls?.Controls?.FirstOrDefault(x => x.Name == name);

            if (control != null)
            {
                ConfigurationControls.Controls.Remove(control);
            }
        }

        protected void AddLabelControl(string name, string label, string text)
        {
            AddControl(UiBuilder.GenerateTextBlock(label, text, "well well-lg", name));
        }
        
        protected void AddControl(ControlDefinitionDTO control)
        {
            ConfigurationControls.Controls.Add(control);
        }

        protected void AddControls(IEnumerable<ControlDefinitionDTO> controls)
        {
            foreach (var controlDefinitionDto in controls)
            {
                ConfigurationControls.Add(controlDefinitionDto);
            }
        }

        protected void AddControls(params ControlDefinitionDTO[] controlsList)
        {
            AddControls((IEnumerable<ControlDefinitionDTO>) controlsList);
        }

        private Crate<StandardConfigurationControlsCM> EnsureControlsCrate()
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