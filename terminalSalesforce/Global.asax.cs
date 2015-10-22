using Data.Infrastructure.AutoMapper;
using System.Web.Http;

namespace terminalSalesforce
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            DataAutoMapperBootStrapper.ConfigureAutoMapper();
            // StructureMap Dependencies configuration
            Core.StructureMap.StructureMapBootStrapper.ConfigureDependencies(Core.StructureMap.StructureMapBootStrapper.DependencyType.LIVE);
        }
    }
}
