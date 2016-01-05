using Hub.StructureMap;
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
                    container.Configure(ConfigureLive); // no test mode yet
                    break;

                case StructureMapBootStrapper.DependencyType.LIVE:
                    container.Configure(ConfigureLive);
                    break;
            }
        }

        /**********************************************************************************/

        private static void ConfigureLive(ConfigurationExpression configurationExpression)
        {
            configurationExpression.For<ITwilioService>().Singleton().Use<TwilioService>();
        }

        /**********************************************************************************/
    }
}
