using System.Net.Http.Formatting;
using System.Web.Http;
using Data.Infrastructure.AutoMapper;
using Data.Infrastructure.StructureMap;
using Data.Repositories;
using Hub.ExternalServices;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Managers.APIManagers.Authorizers;
using Hub.Managers.APIManagers.Authorizers.Google;
using Hub.Managers.APIManagers.Packagers;
using Hub.Managers.APIManagers.Packagers.SegmentIO;
using Hub.Managers.APIManagers.Packagers.SendGrid;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Hub.Managers.APIManagers.Transmitters.Terminal;
using Hub.Security;
using Hub.Services;
using Hub.StructureMap;
using terminalQuickBooks.Interfaces;
using terminalQuickBooks.Services;
using DependencyType = Hub.StructureMap.StructureMapBootStrapper.DependencyType;
using TerminalBase.Infrastructure;
using Utilities;
using Utilities.Interfaces;

namespace terminalQuickBooks
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            StructureMapBootStrapper.ConfigureDependencies(DependencyType.LIVE);
            GlobalConfiguration.Configure(RoutesConfig.Register);
			DataAutoMapperBootStrapper.ConfigureAutoMapper();   
            TerminalBootstrapper.ConfigureLive();
            StructureMapBootStrapper.ConfigureDependencies(DependencyType.LIVE).ConfigureQuickbooksDependencies(DependencyType.LIVE);
        }
    }
}
