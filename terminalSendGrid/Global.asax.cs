using Data.Infrastructure.AutoMapper;
using System.Web;
using System.Web.Http;
using Hub.StructureMap;

namespace terminalSendGrid
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            const StructureMapBootStrapper.DependencyType dependencyType = StructureMapBootStrapper.DependencyType.LIVE;

            GlobalConfiguration.Configure(WebApiConfig.Register);
            DataAutoMapperBootStrapper.ConfigureAutoMapper();

            // StructureMap Dependencies configuration
            StructureMapBootStrapper.ConfigureDependencies(dependencyType).SendGridConfigureDependencies(dependencyType);
        }
    }
}
