using System.Web.Http;
using Data.Infrastructure.AutoMapper;
using Hub.StructureMap;
using TerminalBase.Infrastructure;
using terminalDocuSign.Infrastructure.StructureMap;

using DependencyType = Hub.StructureMap.StructureMapBootStrapper.DependencyType;

namespace terminalDocuSign
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // StructureMap Dependencies configuration
            StructureMapBootStrapper.ConfigureDependencies(DependencyType.LIVE);

            var formatters = GlobalConfiguration.Configuration.Formatters;
            formatters.Remove(formatters.XmlFormatter);

            GlobalConfiguration.Configure(RoutesConfig.Register);
			DataAutoMapperBootStrapper.ConfigureAutoMapper();
            
			TerminalDocuSignMapBootstrapper.ConfigureDependencies(DependencyType.LIVE);
            TerminalBootstrapper.ConfigureLive();
        }
    }
}
