using StructureMap;
using terminalTelegram.TelegramIntegration;

namespace terminalTelegram
{
    public static class TerminalTelegramBootstrapper
    {
        public static void ConfigureTelegramDependencies(this IContainer container)
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
            configurationExpression.For<ITelegramIntegration>().Use<TelegramIntegration.TelegramIntegration>().Singleton();
        }
    }
}