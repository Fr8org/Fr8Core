using System.Web.Http;
using Core.StructureMap;
using Data.Infrastructure.AutoMapper;
using terminal_DocuSign.Infrastructure.AutoMapper;
using terminal_DocuSign.Infrastructure.StructureMap;

using DependencyType = Core.StructureMap.StructureMapBootStrapper.DependencyType;

namespace terminal_DocuSign
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
				DataAutoMapperBootStrapper.ConfigureAutoMapper();
				TerminalDataAutoMapperBootStrapper.ConfigureAutoMapper();
            // StructureMap Dependencies configuration
            StructureMapBootStrapper.ConfigureDependencies(DependencyType.LIVE);
				TerminalDocuSignMapBootstrapper.ConfigureDependencies(DependencyType.LIVE);
        }
    }
}
