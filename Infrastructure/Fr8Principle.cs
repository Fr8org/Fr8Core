using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace HubWeb.Infrastructure
{
    public class Fr8Principle : GenericPrincipal
    {
        private string _terminalId { get; set; }

        public Fr8Principle(string terminalId, IIdentity identity, string[] roles = null) : base(identity, roles)
        {
            _terminalId = terminalId;
        }

        public string GetTerminalId()
        {
            return _terminalId;
        }
    }
}