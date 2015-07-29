using Data.Entities;
using Data.States;

namespace UtilitiesTesting.Fixtures
{
	partial class FixtureData
	{
		public ProcessNodeDO TestProcessNode()
		{
			return new ProcessNodeDO
			{
				Id = 50,
				ParentProcessId = 49,
				Process = new ProcessDO
				{
					Id = 49, 
					CurrentProcessNodeId = 50, 
					ProcessState = 1, 
					ProcessNode = new ProcessNodeDO
					{
						Id = 50, 
						ParentProcessId = 49
					}
				},
				ProcessNodeState = ProcessNodeState.Unstarted
			};
		}
	}
}