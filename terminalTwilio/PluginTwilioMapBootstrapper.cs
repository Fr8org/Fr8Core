using Core.StructureMap;
using terminalTwilio.Services;
using StructureMap;

namespace terminalTwilio
{
    public static class PluginTwilioMapBootstrapper
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
