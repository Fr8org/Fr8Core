using System.Web.Http;
using Data.Infrastructure.AutoMapper;
using TerminalBase.Infrastructure;

namespace terminalSalesforce
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Hub.StructureMap.StructureMapBootStrapper.ConfigureDependencies(Hub.StructureMap.StructureMapBootStrapper.DependencyType.LIVE);
            GlobalConfiguration.Configure(RoutesConfig.Register);
            DataAutoMapperBootStrapper.ConfigureAutoMapper();
            // StructureMap Dependencies configuration
            
            TerminalBootstrapper.ConfigureLive();
        }
    }
}
