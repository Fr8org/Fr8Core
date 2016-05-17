using System.Web.Http;
using terminalPapertrail.Tests.Infrastructure;
using TerminalBase.Infrastructure;
using DependencyType = Fr8Infrastructure.StructureMap.StructureMapBootStrapper.DependencyType;

namespace terminalPapertrail
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            TerminalPapertrailMapBootstrapper.ConfigureDependencies(DependencyType.LIVE);
            TerminalBootstrapper.ConfigureLive();
        }
    }
}
