using System.Security.Principal;

namespace Hub.Infrastructure
{
    public class Fr8Principal : GenericPrincipal
    {
        private string TerminalId { get; set; }

        public Fr8Principal(string terminalId, IIdentity identity, string[] roles = null) : base(identity, roles)
        {
            TerminalId = terminalId;
        }
    }
}