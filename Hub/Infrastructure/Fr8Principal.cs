using System.Security.Principal;

namespace Hub.Infrastructure
{
    public class Fr8Principal : GenericPrincipal
    {
        private string TerminalKey { get; set; }

        public Fr8Principal(string terminalKey, IIdentity identity, string[] roles = null) : base(identity, roles)
        {
            TerminalKey = terminalKey;
        }
    }
}