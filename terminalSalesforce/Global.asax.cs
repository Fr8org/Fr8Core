using System.Web.Http;
using Data.Infrastructure.AutoMapper;

namespace terminalSalesforce
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            DataAutoMapperBootStrapper.ConfigureAutoMapper();
            // StructureMap Dependencies configuration
            Hub.StructureMap.StructureMapBootStrapper.ConfigureDependencies(Hub.StructureMap.StructureMapBootStrapper.DependencyType.LIVE);
        }
    }
}
