namespace Data.States
{
    public class TrackingState
    {
        //General Statuses
        public const int Unprocessed = 1;
        public const int Processed = 2;
        public const int Invalid = 3;


        //BookingRequest Statuses
        public const int Unstarted = 20;
        public const int PendingClarification = 21;
        public const int Completed = 22;
    }
}
