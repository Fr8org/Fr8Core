using System.Security.Principal;

namespace Hub.Infrastructure
{
    public class Fr8Principle : GenericPrincipal
    {
        private string TerminalId { get; set; }

        public Fr8Principle(string terminalId, IIdentity identity, string[] roles = null) : base(identity, roles)
        {
            TerminalId = terminalId;
        }
    }
}