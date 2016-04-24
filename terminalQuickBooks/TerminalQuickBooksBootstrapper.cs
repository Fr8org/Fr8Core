using Hub.Managers;
using Hub.StructureMap;
using StructureMap;
using terminalQuickBooks.Infrastructure;
using terminalQuickBooks.Interfaces;

namespace terminalQuickBooks
{
    public static class TerminalQuickbooksBootstrapper
    {
        public static void ConfigureQuickbooksDependencies(this IContainer container, StructureMapBootStrapper.DependencyType type)
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

        public static void ConfigureLive(ConfigurationExpression configurationExpression)
        {
            configurationExpression.For<IAuthenticator>().Use<Authenticator>();
            configurationExpression.For<ICrateManager>().Use<CrateManager>();
        }
    }
}