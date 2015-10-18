namespace Data.Interfaces.DataTransferObjects
{
    public class AuthTokenDTO
    {
        public string Token { get; set; }
        public string ExternalAccountId { get; set; }
        public string ExternalStateToken { get; set; }

        // Instance URL - Salesforce
        public string ExternalInstanceURL { get; set; }

        // Version URL - Salesforce
        public string ExternalApiVersion { get; set; }

        // Refresh Token - Salesforce
        public string RefreshToken { get; set; }
    }
}
