using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public ActionListDO TestActionList()
        {
            var curActionListDO = new ActionListDO
            {
                Id = 1,
                TemplateId = 1,
                Name = "list1"
            };
            curActionListDO.Actions.Add(TestActionList1());
            curActionListDO.Actions.Add(TestActionList2());

            return curActionListDO;
        }

        public ActionListDO TestEmptyActionList()
        {
            var curActionListDO = new ActionListDO
            {
                Id = 4,
                TemplateId = 1,
                Name = "list1"
            };
            return curActionListDO;
        }
    }
}
