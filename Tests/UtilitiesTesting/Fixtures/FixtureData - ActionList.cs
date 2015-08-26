using Data.Entities;
using Data.States;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static ActionListDO TestActionList()
        {
            var curActionListDO = new ActionListDO
            {
                Id = 1,
                TemplateId = 1,
                Name = "list1",
                ActionListType = ActionListType.Immediate
            };
            curActionListDO.Actions.Add(FixtureData.TestAction1());
            curActionListDO.Actions.Add(FixtureData.TestAction2());

            return curActionListDO;
        }

        public static ActionListDO TestEmptyActionList()
        {
            var curActionListDO = new ActionListDO
            {
                Id = 4,
                TemplateId = 1,
                Name = "list1",
                ActionListType = ActionListType.Immediate
            };
            return curActionListDO;
        }

        public static ActionDO TestActionList1()
        {
            return new ActionDO
            {
                Id = 1,
                UserLabel = "Action 1",
                ActionListId = 1,
                Ordering = 1
            };
        }

        public static ActionListDO TestActionList3()
        {
            return new ActionListDO
            {
                Id = 2,
                CurrentAction = FixtureData.TestAction2(),
                ActionListState = ActionListState.Inprocess
            };
        }

        public static ActionListDO TestActionList4()
        {
            return new ActionListDO
            {
                Id = 2,
                CurrentAction = FixtureData.TestAction6(),
                ActionListState = ActionListState.Unstarted,
                Actions = new System.Collections.Generic.List<ActionDO>() 
                { 
                    FixtureData.TestAction5(),
                    FixtureData.TestAction7(),
                    FixtureData.TestAction8()             
                }
            };
        }

        public static ActionListDO TestActionList5()
        {
            return new ActionListDO
            {
                Id = 2,
                ActionListType = ActionListType.Immediate,
                CurrentAction = FixtureData.TestAction6(),
                ActionListState = ActionListState.Unstarted,
                Actions = new System.Collections.Generic.List<ActionDO>() 
                { 
                    FixtureData.TestAction5(),
                    FixtureData.TestAction7(),
                    FixtureData.TestAction8()             
                }
            };
        }

        public static ActionListDO TestActionList6()
        {
            return new ActionListDO
            {
                Id = 2,
                ActionListType = ActionListType.Immediate,
                ActionListState = ActionListState.Unstarted
            };
        }
    }
}
