using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;

namespace TerminalBase.Infrastructure.Behaviors
{
    public class DropDownListMappingBehavior : BaseControlMappingBehavior<DropDownList>
    {
        public DropDownListMappingBehavior(ICrateStorage crateStorage, string behaviorName) 
            : base(crateStorage, behaviorName)
        {
            BehaviorPrefix = "DropDownListMappingBehavior-";
        }

        public void Append(string labelName, List<ListItem> items)
        {
            var controlsCM = GetOrCreateStandardConfigurationControlsCM();

            var name = string.Concat(BehaviorPrefix, labelName);

            var userDefinedDropDownList = new DropDownList()
            {
                Name = name,
                Label = name,
                ListItems = items
            };

            controlsCM.Controls.Add(userDefinedDropDownList);
        }

        public List<DropDownList> GetValues(ICrateStorage payload = null)
        {
            var controlsCM = GetOrCreateStandardConfigurationControlsCM();

            var dropDownLists = controlsCM
                .Controls.Where(IsBehaviorControl).OfType<DropDownList>();

            return dropDownLists.ToList();
        }
    }
 }
