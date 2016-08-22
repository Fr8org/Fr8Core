namespace Data.States
{

    //DO NOT USE THIS for anything other than Emails. Don't use it for things that inherit from email. 
    public class EmailState
    {
        public const int Queued = 1;
        public const int Sent = 2;
        public const int Unprocessed = 3;
        public const int Processed = 4;
        public const int Dispatched = 5;
        public const int SendRejected = 6;
        public const int SendCriticalError = 7;
        public const int Invalid = 8;
        public const int Unstarted = 9;
    }
}
