using StructureMap;
using terminalAsana.Interfaces;
using terminalAsana.Services;

namespace terminalAsana
{
    public static class TerminalAsanaBootstrapper
    {
        public static void ConfigureAsanaDependencies(this IContainer container)
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
            //configurationExpression.For<IAsanaIntegration>().Use<AsanaIntegrationService>().Singleton();
            configurationExpression.For<IAsanaIntegration>().Use<AsanaIntegrationService>();
        }
    }
}