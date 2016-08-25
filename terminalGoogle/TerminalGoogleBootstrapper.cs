using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.StructureMap;
using StructureMap;
using terminalGoogle.Interfaces;
using terminalGoogle.Services;
using terminalGoogle.Services.Authorization;

namespace terminalGoogle
{
    public static class TerminalGoogleBootstrapper
    {
        public static void ConfigureGoogleDependencies(this StructureMap.IContainer container, StructureMapBootStrapper.DependencyType type)
        {
            switch (type)
            {
                case StructureMapBootStrapper.DependencyType.TEST:
                    container.Configure(ConfigureLive); // no test mode yet
                    break;

                case StructureMapBootStrapper.DependencyType.LIVE:
                    container.Configure(ConfigureLive);
                    break;
            }
        }

        /**********************************************************************************/

        public static void ConfigureLive(ConfigurationExpression configurationExpression)
        {
            configurationExpression.For<IGoogleGmailPolling>().Use<GoogleGmailPolling>();
            configurationExpression.For<IGoogleGDrivePolling>().Use<GoogleGDrivePolling>();
            configurationExpression.For<IGoogleIntegration>().Use<GoogleIntegration>();
            configurationExpression.For<IGoogleDrive>().Use<GoogleDrive>();
            configurationExpression.For<IGoogleSheet>().Use<GoogleSheet>();
            configurationExpression.For<IGoogleAppsScript>().Use<GoogleAppsScript>();
            configurationExpression.For<ICrateManager>().Use<CrateManager>();
        }

    }
}
