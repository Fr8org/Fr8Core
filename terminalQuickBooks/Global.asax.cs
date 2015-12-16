using System.Web.Http;
using Data.Infrastructure.AutoMapper;
using Hub.StructureMap;
using DependencyType = Hub.StructureMap.StructureMapBootStrapper.DependencyType;
using TerminalBase.Infrastructure;

namespace terminalQuickBooks
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
			DataAutoMapperBootStrapper.ConfigureAutoMapper();
            // StructureMap Dependencies configuration
            StructureMapBootStrapper.ConfigureDependencies(DependencyType.LIVE);
            TerminalBootstrapper.ConfigureLive();
        }
    }
}
