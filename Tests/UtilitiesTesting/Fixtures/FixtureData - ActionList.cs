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
                CurrentActivity = TestAction21(),
                ActionListState = ActionListState.Inprocess
            };
        }

        public static ActionListDO TestActionList4()
        {
            return new ActionListDO
            {
                Id = 2,
                CurrentActivity = TestAction21(),
                ActionListState = ActionListState.Unstarted
            };
        }

        public static ActionListDO TestActionList5()
        {
            return new ActionListDO
            {
                Id = 2,
                ActionListType = ActionListType.Immediate,
                CurrentActivity = FixtureData.TestAction6(),
                ActionListState = ActionListState.Unstarted,
                Actions = new System.Collections.Generic.List<ActionDO>() 
                { 
                    FixtureData.TestAction10(),
                    FixtureData.TestAction7(),
                    FixtureData.TestAction8()             
                }
            };
        }

        public static ActionListDO TestActionList6()
        {
            ProcessDO processDO = FixtureData.TestProcess1();
            processDO.EnvelopeId = "";
            return new ActionListDO
            {
                Id = 2,
                ActionListType = ActionListType.Immediate,
                ActionListState = ActionListState.Unstarted,
                Process = processDO
            };
        }

        public static ActionListDO TestActionList7()
        {
            return new ActionListDO
            {
                Id = 2,
                CurrentActivity = FixtureData.TestAction6(),
                ActionListState = ActionListState.Unstarted,
                Actions = new System.Collections.Generic.List<ActionDO>() 
                { 
                    FixtureData.TestAction10(),
                    FixtureData.TestAction7(),
                    FixtureData.TestAction8()             
                }
            };
        }
    }
}
