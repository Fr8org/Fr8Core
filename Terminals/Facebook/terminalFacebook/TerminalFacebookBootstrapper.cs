using StructureMap;
using terminalFacebook.Interfaces;
using terminalFacebook.Services;

namespace terminalFacebook
{
    public static class TerminalFacebookBootstrapper
    {
        public static void ConfigureSlackDependencies(this IContainer container)
        {
            container.Configure(ConfigureLive);
        }

        /**********************************************************************************/

        public static void ConfigureLive(ConfigurationExpression configurationExpression)
        {
            configurationExpression.For<IFacebookIntegration>().Use<FacebookIntegration>().Singleton();
            configurationExpression.For<IEvent>().Use<Event>();
        }
    }
}