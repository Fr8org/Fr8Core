using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.StructureMap;
using StructureMap;
using terminalStatX.Interfaces;
using terminalStatX.Services;

namespace terminalStatX
{
    public static class TerminalStatXBootstrapper
    {
        public static void ConfigureStatXDependencies(this IContainer container, StructureMapBootStrapper.DependencyType type)
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
            configurationExpression.For<IStatXIntegration>().Use<StatXIntegration>();
            configurationExpression.For<IStatXPolling>().Use<StatXPolling>();
            configurationExpression.For<ICrateManager>().Use<CrateManager>();
        }
    }
}