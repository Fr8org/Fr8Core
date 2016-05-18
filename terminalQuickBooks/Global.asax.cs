using System.Net.Http.Formatting;
using System.Web.Http;
using terminalQuickBooks.Interfaces;
using terminalQuickBooks.Services;
using TerminalBase.Infrastructure;
using Utilities;
using Utilities.Interfaces;

namespace terminalQuickBooks
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            StructureMapBootStrapper.ConfigureDependencies(DependencyType.LIVE);
            GlobalConfiguration.Configure(RoutesConfig.Register);
			DataAutoMapperBootStrapper.ConfigureAutoMapper();   
            TerminalBootstrapper.ConfigureLive();
            StructureMapBootStrapper.ConfigureDependencies(DependencyType.LIVE).ConfigureQuickbooksDependencies(DependencyType.LIVE);
        }
    }
}
