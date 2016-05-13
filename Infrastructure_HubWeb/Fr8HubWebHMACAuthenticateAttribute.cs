using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Filters;
using Hub.Infrastructure;
using Hub.Interfaces;
using Infrastructure.Security;
using StructureMap;

namespace HubWeb.Infrastructure_HubWeb
{
    public class Fr8HubWebHMACAuthenticateAttribute : fr8HMACAuthenticateAttribute
    {
        public Fr8HubWebHMACAuthenticateAttribute()
        {
            _terminalService = ObjectFactory.GetInstance<ITerminal>();
        }

        private readonly ITerminal _terminalService;

        protected override async Task<string> GetTerminalSecret(string terminalId)
        {
            var terminal = await _terminalService.GetTerminalByPublicIdentifier(terminalId);
            if (terminal == null)
            {
                return null;
            }

            return terminal.Secret;
        }

        protected override async Task<bool> CheckPermission(string terminalId, string userId)
        {
            var terminal = await _terminalService.GetTerminalByPublicIdentifier(terminalId);
            if (terminal == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(userId))
            {
                //hmm think about this
                //TODO with a empty userId a terminal can only call single Controller
                //which is OpsController?
                //until we figure out exceptions, we won't allow this
                return false;
            }

            //TODO discuss and enable this
            /*
            //let's check if user allowed this terminal to modify it's data
            if (!await _terminalService.IsUserSubscribedToTerminal(terminalId, userId))
            {
                return false;
            }
             * */

            return true;
        }

        protected override void Success(HttpAuthenticationContext context, string terminalId, string userId)
        {
            var identity = new Fr8Identity("terminal-" + terminalId, userId);
            var principle = new Fr8Principle(terminalId, identity, new [] { "Terminal" });
            Thread.CurrentPrincipal = principle;
            context.Principal = principle;
            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principle;
            }

        }
    }
}
   
