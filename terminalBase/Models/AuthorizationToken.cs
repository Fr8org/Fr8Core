using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalBase.Models
{
    public class AuthorizationToken
    {
        public string Id { get; set; }
        public string Token { get; set; }
        public string ExternalAccountId { get; set; }
        public string ExternalDomainId { get; set; }
        public string ExternalAccountName { get; set; }
        public string ExternalDomainName { get; set; }
        public string UserId { get; set; }
        public string ExternalStateToken { get; set; }
        public string AdditionalAttributes { get; set; }
        public string Error { get; set; }
        public bool AuthCompletedNotificationRequired { get; set; }
        public int TerminalID { get; set; }
    }
}
