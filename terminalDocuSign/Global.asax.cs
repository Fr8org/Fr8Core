using System.Web.Http;
using Core.StructureMap;
using Data.Infrastructure.AutoMapper;
using terminalDocuSign.Infrastructure.AutoMapper;
using terminalDocuSign.Infrastructure.StructureMap;

using DependencyType = Core.StructureMap.StructureMapBootStrapper.DependencyType;

namespace terminalDocuSign
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
				DataAutoMapperBootStrapper.ConfigureAutoMapper();
				PluginDataAutoMapperBootStrapper.ConfigureAutoMapper();
            // StructureMap Dependencies configuration
            StructureMapBootStrapper.ConfigureDependencies(DependencyType.LIVE);
				PluginDocuSignMapBootstrapper.ConfigureDependencies(DependencyType.LIVE);
        }
    }
}
