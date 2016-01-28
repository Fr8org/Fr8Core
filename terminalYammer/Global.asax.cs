using System.Web.Http;
using Data.Infrastructure.AutoMapper;
using Hub.StructureMap;
using TerminalBase.Infrastructure;

namespace terminalYammer
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.LIVE);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            
            DataAutoMapperBootStrapper.ConfigureAutoMapper();
            TerminalBootstrapper.ConfigureLive();
        }
    }
}
