using Data.Entities;
using Data.States;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {

        public static ActionListDO TestActionListHealth1()
        {
            return new ActionListDO
            {
                Id = 88,
                Name = "list1",
                ActionListType = ActionListType.Immediate,
                ProcessNodeTemplateID = 50
            };

        }


        public static ActionListDO TestActionList()
        {
            var curActionListDO = new ActionListDO
            {
                Id = 1,
                ProcessNodeTemplateID = 1,
                Name = "list1",
                ActionListType = ActionListType.Immediate
            };
            curActionListDO.Actions.Add(TestAction20());
            curActionListDO.Actions.Add(TestAction21());

            return curActionListDO;
        }

        public static ActionListDO TestEmptyActionList()
        {
            var curActionListDO = new ActionListDO
            {
                Id = 4,
                ProcessNodeTemplateID = 1,
                Name = "list1",
                ActionListType = ActionListType.Immediate
            };
            return curActionListDO;
        }

      

        public static ActionListDO TestActionListMedical()
        {
            var curActionListDO = new ActionListDO
            {
                Id = 4,
                ProcessNodeTemplateID = 1,
                Name = "list1",
                ActionListType = ActionListType.Immediate,                    
            };
            return curActionListDO;
        }

        public static ActionListDO TestActionList3()
        {
            return new ActionListDO
            {
                Id = 2,
                CurrentAction = TestAction21(),
                ActionListState = ActionListState.Inprocess
            };
        }

        public static ActionListDO TestActionList4()
        {
            return new ActionListDO
            {
                Id = 2,
                CurrentAction = TestAction21(),
                ActionListState = ActionListState.Unstarted
            };
        }
    }
}
