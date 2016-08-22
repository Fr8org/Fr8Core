using System.Collections.Generic;
using System.Linq;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;

namespace Fr8.TerminalBase.Infrastructure.Behaviors
{
    public class DropDownListMappingBehavior : BaseControlMappingBehavior<DropDownList>
    {
        public DropDownListMappingBehavior(ICrateStorage crateStorage, string behaviorName) 
            : base(crateStorage, behaviorName)
        {
           // BehaviorPrefix = "DropDownListMappingBehavior-";
        }

        public void Append(string name, string labelName,  List<ListItem> items)
        {
            var controlsCM = GetOrCreateStandardConfigurationControlsCM();

            var theName = string.Concat(BehaviorPrefix, name);

            var userDefinedDropDownList = new DropDownList()
            {
                Name = theName,
                Label = labelName,
                ListItems = items
            };

            //set selected key to first item
            var firstItem = items.FirstOrDefault();
            if (firstItem != null)
            {
                userDefinedDropDownList.selectedKey = firstItem.Key;
            }

            controlsCM.Controls.Add(userDefinedDropDownList);
        }

        public List<DropDownList> GetValues(ICrateStorage payload = null)
        {
            var controlsCM = GetOrCreateStandardConfigurationControlsCM();

            var dropDownLists = controlsCM
                .Controls.Where(IsBehaviorControl).OfType<DropDownList>();
            var resultDropDownCollection = new List<DropDownList>();
            foreach (var list in dropDownLists)
            {
                list.Name = GetFieldId(list);
                resultDropDownCollection.Add(list);
            }

            return resultDropDownCollection;
        }
    }
 }
