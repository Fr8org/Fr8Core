namespace Data.States
{
	public class ProcessNodeState
	{
		public const int Unstarted = 1;
		public const int EvaluatingCriteria = 2;
		public const int ProcessingActions = 3;
		public const int Complete = 4;
		public const int Error = 5;
	}
}