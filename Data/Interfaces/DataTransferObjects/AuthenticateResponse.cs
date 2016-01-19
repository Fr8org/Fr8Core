using Data.Entities;

namespace Data.Interfaces.DataTransferObjects
{
    public class AuthenticateResponse
    {
        public AuthorizationTokenDO AuthorizationToken { get; set; }

        public string Error { get; set; }
    }
}
