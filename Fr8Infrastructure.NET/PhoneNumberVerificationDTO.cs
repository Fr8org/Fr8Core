namespace Fr8.Infrastructure
{
    public class PhoneNumberVerificationDTO
    {
        public int TerminalId { get; set; }

        public string TerminalName { get; set; }

        public string ClientId { get; set; }

        public string ClientName { get; set; }

        public string PhoneNumber { get; set; }

        public string Error { get; set; }

        public string Message { get; set; }
    }
}
