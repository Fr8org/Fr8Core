namespace Data.States
{
	public class State
	{
		public const int Unstarted = 1;
		public const int Executing = 2;
		public const int Completed = 4;
		public const int Failed = 5;
        public const int Suspended = 6;
        public const int Deleted = 7;
	}
}