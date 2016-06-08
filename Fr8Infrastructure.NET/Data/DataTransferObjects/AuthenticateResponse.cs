
namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class AuthenticateResponse
    {
        public AuthorizationTokenDTO AuthorizationToken { get; set; }

        public string Error { get; set; }
    }
}
