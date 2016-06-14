
namespace Fr8Data.DataTransferObjects
{
    public class AuthenticateResponse
    {
        public AuthorizationTokenDTO AuthorizationToken { get; set; }

        public string Error { get; set; }
    }
}
