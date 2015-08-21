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
            curActionListDO.Actions.Add(TestActionList1());
            curActionListDO.Actions.Add(TestActionList2());

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

        public static ActionDO TestActionList2()
        {
            return new ActionDO
            {
                Id = 2,
                UserLabel = "Action 2",
                ActionListId = 1,
                Ordering = 2
            };
        }

        public static ActionListDO TestActionList3()
        {
            return new ActionListDO
            {
                Id = 2,
                CurrentAction = TestActionList2(),
                ActionListState = ActionListState.Inprocess
            };
        }

        public static ActionListDO TestActionList4()
        {
            return new ActionListDO
            {
                Id = 2,
                CurrentAction = TestActionList2(),
                ActionListState = ActionListState.Unstarted
            };
        }
    }
}
