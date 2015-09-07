using Data.Entities;
using Data.States;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {

        public static ActionListDO TestActionListHealth1()
        {
            string envelopeId = "F02C3D55-F6EF-4B2B-B0A0-02BF64CA1E09";
            var processDo = new ProcessDO
            {
                Id = 1,
                EnvelopeId = envelopeId,
                ProcessState = 1,
                Name = "test name",
                ProcessTemplateId = TestProcessTemplateHealthDemo().Id
            };

            return new ActionListDO
            {
                Id = 88,
                Name = "list1",
                ActionListType = ActionListType.Immediate,
                ProcessNodeTemplateID = 50,
                CurrentActivity = TestActionHealth1(),
                Process = processDo
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
            curActionListDO.Activities.Add(TestAction20());
            curActionListDO.Activities.Add(TestAction21());

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
                Activities = new System.Collections.Generic.List<ActivityDO>() 
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
                Activities = new System.Collections.Generic.List<ActivityDO>() 
                { 
                    FixtureData.TestAction10(),
                    FixtureData.TestAction7(),
                    FixtureData.TestAction8()             
                }
            };
        }
    }
}
