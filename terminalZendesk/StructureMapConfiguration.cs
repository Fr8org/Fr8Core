using Fr8.Infrastructure.StructureMap;
using Fr8.Infrastructure.Utilities;
using StructureMap;

namespace terminalZendesk
{
    public static class TerminalZendeskStructureMapBootstrapper
    {
        public class LiveMode : StructureMapBootStrapper.LiveMode
        {
            public LiveMode()
            {
                //For<IEmailPackager>().Use<SendGridPackager>();
                //For<ITransport>().Use(c => TransportFactory.CreateWeb(c.GetInstance<IConfigRepository>()));
            }
        }

        public static void LiveConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<LiveMode>();
        }
    }
}
