using System.Collections.Generic;
using System.Linq;
using Fr8Data.Control;
using Fr8Data.Crates;

namespace TerminalBase.Infrastructure.Behaviors
{
    public class CheckBoxMappingBehavior : BaseControlMappingBehavior<CheckBox>
    {
        public CheckBoxMappingBehavior(ICrateStorage crateStorage, string behaviorName) 
            : base(crateStorage, behaviorName)
        {
           // BehaviorPrefix = behaviorName;
        }

        public void Append(string fieldId, string label)
        {
            var controlsCM = GetOrCreateStandardConfigurationControlsCM();

            var name = string.Concat(BehaviorPrefix, fieldId);

            var textSource = new CheckBox()
            {
                Label = label,
                Name = name
            };
            controlsCM.Controls.Add(textSource);
        }

        public List<CheckBox> GetValues(ICrateStorage payload = null)
        {
            var controlsCM = GetOrCreateStandardConfigurationControlsCM();

            var checkBoxes = controlsCM
                .Controls.Where(IsBehaviorControl).OfType<CheckBox>();
            var checkBoxesResult = new List<CheckBox>();
            foreach (var checkBox in checkBoxes)
            {
                checkBox.Name = GetFieldId(checkBox);
                checkBoxesResult.Add(checkBox);
            }

            return checkBoxesResult;
        }
    }
}
