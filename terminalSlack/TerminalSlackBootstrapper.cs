using Hub.StructureMap;
using StructureMap;
using terminalSlack.Interfaces;
using terminalSlack.Services;

namespace terminalSlack
{
    public static class TerminalSlackBootstrapper
    {
        public static void ConfigureSlackDependencies(this IContainer container, StructureMapBootStrapper.DependencyType type)
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
            configurationExpression.For<ISlackEventManager>().Use<SlackEventManager>().Singleton();
            configurationExpression.For<ISlackIntegration>().Use<SlackIntegration>().Singleton();
        }
    }
}