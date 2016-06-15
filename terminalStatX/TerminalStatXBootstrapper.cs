using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.StructureMap;
using StructureMap;

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
            configurationExpression.For<ICrateManager>().Use<CrateManager>();
        }
    }
}