using StructureMap;
using terminalSlack.Interfaces;
using terminalSlack.Services;

namespace terminalSlack
{
    public static class TerminalSlackBootstrapper
    {
        public static void ConfigureSlackDependencies(this IContainer container)
        {
            /*
            switch (type)
            {
                case StructureMapBootStrapper.TEST:
                    container.Configure(ConfigureLive); // no test mode yet
                    break;

                case StructureMapBootStrapper.DependencyType.LIVE:
                    container.Configure(ConfigureLive);
                    break;
            }*/
            container.Configure(ConfigureLive);
        }

        /**********************************************************************************/

        public static void ConfigureLive(ConfigurationExpression configurationExpression)
        {
            configurationExpression.For<ISlackEventManager>().Use<SlackEventManager>().Singleton();
            configurationExpression.For<ISlackIntegration>().Use<SlackIntegration>().Singleton();
        }
    }
}