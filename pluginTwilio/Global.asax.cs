using System.Web;
using System.Web.Http;
using Core.StructureMap;
using Data.Infrastructure.AutoMapper;

namespace pluginTwilio
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            const StructureMapBootStrapper.DependencyType dependencyType = StructureMapBootStrapper.DependencyType.LIVE;

            GlobalConfiguration.Configure(WebApiConfig.Register);
            DataAutoMapperBootStrapper.ConfigureAutoMapper();
            // StructureMap Dependencies configuration
            StructureMapBootStrapper.ConfigureDependencies(dependencyType).ConfigureTwilioDependencies(dependencyType);
        }
    }
}
