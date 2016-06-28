using StructureMap;
using terminalMailChimp.Interfaces;
using terminalMailChimp.Services;

namespace terminalMailChimp
{
    public static class TerminalMailChimpBootstrapper
    {
        public static void ConfigureLive(ConfigurationExpression configurationExpression)
        {
            configurationExpression.For<IMailChimpIntegration>().Use<MailChimpIntegration>().Singleton();
        }
    }
}