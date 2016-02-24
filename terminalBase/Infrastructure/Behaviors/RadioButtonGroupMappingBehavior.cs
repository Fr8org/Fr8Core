using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using StructureMap;

namespace TerminalBase.Infrastructure.Behaviors
{
    public class RadioButtonGroupMappingBehavior
    {
        public const string ConfigurationControlsLabel = "Configuration_Controls";
        public const string BehaviorPrefix = "RadioButtonGroupMappingBehavior-";

        private ICrateManager _crateManager;
        private ICrateStorage _crateStorage;
   
        public RadioButtonGroupMappingBehavior(
            ICrateStorage crateStorage)
        {
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
            _crateStorage = crateStorage;
        }

        public ICrateStorage CrateStorage
        {
            get { return _crateStorage; }
        }

        private StandardConfigurationControlsCM GetOrCreateStandardConfigurationControlsCM()
        {
            var controlsCM = _crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>()
                .FirstOrDefault();

            if (controlsCM == null)
            {
                var crate = _crateManager.CreateStandardConfigurationControlsCrate(ConfigurationControlsLabel);
                _crateStorage.Add(crate);

                controlsCM = crate.Content;
            }

            return controlsCM;
        }

        private bool IsBehaviorControl(ControlDefinitionDTO control)
        {
            return control.Name != null && control.Name.StartsWith(BehaviorPrefix);
        }

        private string GetFieldId(ControlDefinitionDTO control)
        {
            return control.Name.Substring(BehaviorPrefix.Length);
        }

        public void Clear()
        {
            var controlsCM = GetOrCreateStandardConfigurationControlsCM();

            var radioButtonGroups = controlsCM
                .Controls.Where(IsBehaviorControl).OfType<RadioButtonGroup>().ToList();

            foreach (var radioButtonGroup in radioButtonGroups)
            {
                controlsCM.Controls.Remove(radioButtonGroup);
            }
        }

        public void Append(List<GroupWrapperEnvelopeDataDTO> radioButtonEnvelopeDataCollection)
        {
            var controlsCM = GetOrCreateStandardConfigurationControlsCM();

            foreach (var item in radioButtonEnvelopeDataCollection)
            {
                var name = string.Concat(BehaviorPrefix, item.Name);

                var radioButtonGroup = CreateUserDefinedRadioButtonGroup(name, item);
                controlsCM.Controls.Add(radioButtonGroup);
            }
        }

        private RadioButtonGroup CreateUserDefinedRadioButtonGroup(string name, GroupWrapperEnvelopeDataDTO radioButtonEnvelopeData)
        {
            var userDefinedRadioButtonGroup = new RadioButtonGroup()
            {
                GroupName = name,
                Name = name,
                Label = name,
                Radios = new List<RadioButtonOption>()
            };

            foreach (var item in radioButtonEnvelopeData.Items)
            {
                userDefinedRadioButtonGroup.Radios.Add(new RadioButtonOption
                {
                    Value = item.Value,
                    Name = item.Value,
                    Selected = item.Selected
                });
            }

            return userDefinedRadioButtonGroup;
        }

        public List<RadioButtonGroup> GetValues(ICrateStorage payload = null)
        {
            var controlsCM = GetOrCreateStandardConfigurationControlsCM();
            var result = new Dictionary<string, string>();

            var radioButtonGroups = controlsCM
                .Controls.Where(IsBehaviorControl).OfType<RadioButtonGroup>();

            //foreach (var radioButtonGroup in radioButtonGroups)
            //{
            //    var fieldId = GetFieldId(radioButtonGroup);
            //    string value = null;
            //    try
            //    {
            //        value = radioButtonGroup.GetValue(payload ?? _crateStorage);
            //    }
            //    catch { }
            //    result.Add(fieldId, value);
            //}

            return radioButtonGroups.ToList();
        }

    }
}
