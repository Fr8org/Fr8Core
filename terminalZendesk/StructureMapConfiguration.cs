using Fr8.Infrastructure.StructureMap;
using Fr8.Infrastructure.Utilities;
using StructureMap;
using terminalZendesk.Interfaces;
using terminalZendesk.Services;

namespace terminalZendesk
{
    public static class TerminalZendeskStructureMapBootstrapper
    {
        public class LiveMode : StructureMapBootStrapper.LiveMode
        {
            public LiveMode()
            {
                For<IZendeskIntegration>().Use<ZendeskIntegration>();
                //For<ITransport>().Use(c => TransportFactory.CreateWeb(c.GetInstance<IConfigRepository>()));
            }
        }

        public static void LiveConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<LiveMode>();
        }
    }
}
