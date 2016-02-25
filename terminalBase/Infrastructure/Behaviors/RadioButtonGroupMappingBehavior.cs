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
        private ICrateManager _crateManager;
        private ICrateStorage _crateStorage;
   
        public RadioButtonGroupMappingBehavior(ICrateStorage crateStorage, string behaviorName) 
            : base(crateStorage, behaviorName)
        {
            BehaviorPrefix = "RadioButtonGroupMappingBehavior-";
        }
        
        public void Append(string labelName, List<RadioButtonOption> radios )
        {
            var controlsCM = GetOrCreateStandardConfigurationControlsCM();

            var name = string.Concat(BehaviorPrefix, labelName);

            var userDefinedRadioButtonGroup = new RadioButtonGroup()
            {
                GroupName = name,
                Name = name,
                Label = name,
                Radios = radios
            };

            controlsCM.Controls.Add(userDefinedRadioButtonGroup);
        }

        public List<RadioButtonGroup> GetValues(ICrateStorage payload = null)
        {
            var controlsCM = GetOrCreateStandardConfigurationControlsCM();

            var radioButtonGroups = controlsCM
                .Controls.Where(IsBehaviorControl).OfType<RadioButtonGroup>();

            return radioButtonGroups.ToList();
        }
    }
}
