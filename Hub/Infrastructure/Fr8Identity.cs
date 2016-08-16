using System.Security.Claims;

namespace Hub.Infrastructure
{
    public sealed class Fr8Identity : ClaimsIdentity
    {
        public Fr8Identity(string terminalKey) : base("FR8")
        {
            AddClaim(new Claim("TerminalKey", terminalKey));
        }
    }
}
