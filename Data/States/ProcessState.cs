namespace Data.States
{
    public class ProcessState
    {
        public const int Unstarted = 1;
        public const int Processing = 2;
        public const int WaitingForPlugin = 3;
        public const int Completed = 4;
        public const int Failed = 5;
    }
}
