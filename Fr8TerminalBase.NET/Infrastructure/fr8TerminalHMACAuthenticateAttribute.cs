using System.Configuration;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Filters;
using Fr8.Infrastructure.Security;
using Fr8.Infrastructure.Utilities.Configuration;

namespace Fr8.TerminalBase.Infrastructure
{
    public class fr8TerminalHMACAuthenticateAttribute : fr8HMACAuthenticateAttribute
    {
        protected string TerminalSecret { get; set; }
        protected string TerminalId { get; set; }
        public fr8TerminalHMACAuthenticateAttribute(string terminalName)
        {
            TerminalSecret = CloudConfigurationManager.GetSetting("TerminalSecret");
            TerminalId = CloudConfigurationManager.GetSetting("TerminalId");

            //we might be on integration test currently
            if (TerminalSecret == null || TerminalId == null)
            {
                TerminalSecret = ConfigurationManager.AppSettings[terminalName + "TerminalSecret"];
                TerminalId = ConfigurationManager.AppSettings[terminalName + "TerminalId"];
            }
        }

        protected override void Success(HttpAuthenticationContext context, string terminalId, string userId)
        {
            var identity = new GenericIdentity("terminal-" + terminalId, userId);
            var principle = new GenericPrincipal(identity, new string[] { });
            Thread.CurrentPrincipal = principle;
            context.Principal = principle;
            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principle;
            }
        }

        protected override async Task<string> GetTerminalSecret(string terminalId)
        {
            if (terminalId == TerminalId)
            {
                return TerminalSecret;
            }

            return null;
        }

        protected override Task<bool> CheckPermission(string terminalId, string userId)
        {
            return Task.FromResult(true);
        }
    }
}