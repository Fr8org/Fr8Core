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
                Name = "test name"
            };

            return new ActionListDO
            {
                Id = 88,
                Name = "list1",
                ActionListType = ActionListType.Immediate,
                ProcessNodeTemplateID = 50,
                CurrentAction = TestActionHealth1(),
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
