namespace Data.Interfaces.DataTransferObjects
{
    public class AuthTokenDTO
    {
        public string Token { get; set; }
        public string ExternalAccountId { get; set; }
        public string ExternalStateToken { get; set; }
        public string AdditionalAttributes { get; set; }
    }
}
