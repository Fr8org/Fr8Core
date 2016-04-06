namespace terminalDocuSign.DataTransferObjects
{
    public class DocuSignAuthTokenDTO
    {
        public string Email { get; set; }
        public string ApiPassword { get; set; }
        public string AccountId { get; set; }
        public bool IsDemoAccount { get; set; }
        public string Endpoint { get; set; }
    }
}