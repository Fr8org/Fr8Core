using StructureMap;
using terminalInstagram.Interfaces;
using terminalInstagram.Services;

namespace terminalInstagram
{
    public static class TerminalInstagramBootstrapper
    {
        public static void ConfigureSlackDependencies(this IContainer container)
        {
            container.Configure(ConfigureLive);
        }

        /**********************************************************************************/

        public static void ConfigureLive(ConfigurationExpression configurationExpression)
        {
            configurationExpression.For<IInstagramIntegration>().Use<InstagramIntegration>();
            configurationExpression.For<IInstagramEventManager>().Use<InstagramEventManager>();
        }
    }
}