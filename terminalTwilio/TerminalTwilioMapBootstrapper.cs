using Fr8Infrastructure.StructureMap;
using StructureMap;
using terminalTwilio.Services;

namespace terminalTwilio
{
    public static class TerminalTwilioMapBootstrapper
    {
        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

        public static void ConfigureTwilioDependencies(this IContainer container, StructureMapBootStrapper.DependencyType type)
        {
            switch (type)
            {
                case StructureMapBootStrapper.DependencyType.TEST:
                    container.Configure(LiveConfiguration); // no test mode yet
                    break;

                case StructureMapBootStrapper.DependencyType.LIVE:
                    container.Configure(LiveConfiguration);
                    break;
            }
        }

        /**********************************************************************************/

        public static void LiveConfiguration(ConfigurationExpression configurationExpression)
        {
            configurationExpression.For<ITwilioService>().Singleton().Use<TwilioService>();
        }

        /**********************************************************************************/
    }
}
