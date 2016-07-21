namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class TokenResponseDTO
    {
        public int? TerminalId { get; set; }

        public string TerminalName { get; set; }

        public string AuthTokenId { get; set; }
        
        public string Error { get; set; }
    }
}
