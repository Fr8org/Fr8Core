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
	}
}