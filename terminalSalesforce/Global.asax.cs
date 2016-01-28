using System.Web.Http;
using Data.Infrastructure.AutoMapper;
using TerminalBase.Infrastructure;

namespace terminalSalesforce
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(RoutesConfig.Register);
            DataAutoMapperBootStrapper.ConfigureAutoMapper();
            // StructureMap Dependencies configuration
            Hub.StructureMap.StructureMapBootStrapper.ConfigureDependencies(Hub.StructureMap.StructureMapBootStrapper.DependencyType.LIVE);
            TerminalBootstrapper.ConfigureLive();
        }
    }
}
