using Data.Entities;
using Data.States;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static ProcessNodeDO TestProcessNode()
        {
            var processNode = new ProcessNodeDO();
            processNode.Id = 50;
            processNode.ParentProcessId = 49;
            processNode.ProcessNodeTemplateId = 50;
            processNode.ProcessNodeTemplate = TestProcessNodeTemplateDO1();
            processNode.ProcessNodeState = ProcessNodeState.Unstarted;
            processNode.ParentProcess = TestProcess1();

            return processNode;
        }

        public static ProcessNodeDO TestProcessNode1()
        {
            var processNode = new ProcessNodeDO();
            processNode.Id = 50;
            processNode.ParentProcessId = 49;

            return processNode;
        }

        public static ProcessNodeDO TestProcessNode2()
        {

            var processNode = new ProcessNodeDO();
            processNode.Id = 51;
            processNode.ParentProcessId = 49;
            processNode.ProcessNodeTemplate = TestProcessNodeTemplateDO1();
            processNode.ProcessNodeTemplate.ActionLists.Add(TestActionList5());

            return processNode;
        }

        public static ProcessNodeDO TestProcessNode3()
        {
            var processNode = new ProcessNodeDO();
            processNode.Id = 51;
            processNode.ParentProcessId = 49;
            processNode.ProcessNodeTemplate = TestProcessNodeTemplateDO2();
            processNode.ProcessNodeTemplate.ActionLists.Add(TestActionList5());

            return processNode;
        }

        public static ProcessNodeDO TestProcessNode4()
        {

            var processNode = new ProcessNodeDO();
            processNode.Id = 1;
            processNode.ParentProcessId = 49;
            processNode.ProcessNodeTemplateId = 50;
            processNode.ProcessNodeTemplate = TestProcessNodeTemplateDO3();
            processNode.ProcessNodeTemplate.ActionLists.Add(TestActionList6());

            return processNode;
        }
    }
}