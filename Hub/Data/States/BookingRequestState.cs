namespace Data.States
{
    public class BookingRequestState
    {
        public const int Resolved = 2;
        public const int Booking = 3;
        public const int Invalid = 4;
        public const int AwaitingClient = 5;
        public const int NeedsBooking = 6;
        public const int Finished = 7;
    }
}
