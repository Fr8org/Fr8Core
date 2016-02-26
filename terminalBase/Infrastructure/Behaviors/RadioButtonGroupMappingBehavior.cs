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
    public class RadioButtonGroupMappingBehavior : BaseControlMappingBehavior<RadioButtonGroup>
    {
        public RadioButtonGroupMappingBehavior(ICrateStorage crateStorage, string behaviorName) 
            : base(crateStorage, behaviorName)
        {
            BehaviorPrefix = "RadioButtonGroupMappingBehavior-";
        }
        
        public void Append(string name, string label, List<RadioButtonOption> radios )
        {
            var controlsCM = GetOrCreateStandardConfigurationControlsCM();

            var mappingName = string.Concat(BehaviorPrefix, name);

            var userDefinedRadioButtonGroup = new RadioButtonGroup()
            {
                GroupName = mappingName,
                Name = mappingName,
                Label = string.IsNullOrEmpty(label) ? name : label,
                Radios = radios
            };

            controlsCM.Controls.Add(userDefinedRadioButtonGroup);
        }

        public List<RadioButtonGroup> GetValues(ICrateStorage payload = null)
        {
            var controlsCM = GetOrCreateStandardConfigurationControlsCM();

            var radioButtonGroups = controlsCM
                .Controls.Where(IsBehaviorControl).OfType<RadioButtonGroup>();

            foreach (var rbGroup in radioButtonGroups)
            {
                rbGroup.GroupName = GetFieldId(rbGroup);
            }

            return radioButtonGroups.ToList();
        }
    }
}
