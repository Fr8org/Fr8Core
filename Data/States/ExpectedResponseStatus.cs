namespace Data.States
{
    public class ExpectedResponseStatus
    {
        public const int Active = 1;
        public const int ResponseReceived = 2;
        public const int Stale = 3;
        public const int Closed = 4;
    }
}
