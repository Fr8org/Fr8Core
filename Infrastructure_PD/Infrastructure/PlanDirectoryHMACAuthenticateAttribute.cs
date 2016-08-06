//using System.Configuration;
//using System.Security.Principal;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Web;
//using System.Web.Http.Filters;
//using Fr8.Infrastructure.Security;
//using Fr8.Infrastructure.Utilities.Configuration;
//using Hub.Infrastructure;

//namespace PlanDirectory.Infrastructure
//{
//    public class PlanDirectoryHMACAuthenticateAttribute : fr8HMACAuthenticateAttribute
//    {
//        private const string TerminalId = "PlanDirectory";


//        public PlanDirectoryHMACAuthenticateAttribute()
//        {
//        }

//        protected override void Success(HttpAuthenticationContext context, string terminalId, string userId)
//        {
//            var identity = new Fr8Identity("terminal-" + terminalId, userId);
//            var principle = new GenericPrincipal(identity, new string[] { });

//            Thread.CurrentPrincipal = principle;
//            context.Principal = principle;

//            if (HttpContext.Current != null)
//            {
//                HttpContext.Current.User = principle;
//            }
//        }

//        protected override Task<string> GetTerminalSecret(string terminalId)
//        {
//            if (terminalId == TerminalId)
//            {
//                return Task.FromResult(CloudConfigurationManager.GetSetting("PlanDirectorySecret"));
//            }

//            return Task.FromResult<string>(null);
//        }

//        protected override Task<bool> CheckPermission(string terminalId, string userId)
//        {
//            return Task.FromResult(true);
//        }
//    }
//}