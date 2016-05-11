using System.Security.Claims;

namespace HubWeb.Infrastructure_HubWeb
{
    public sealed class Fr8Identity : ClaimsIdentity
    {
        public Fr8Identity(string name, string userId) : base ("hmac")
        {
            AddClaim(new Claim(ClaimTypes.Name, name));
            AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
        }
    }
}