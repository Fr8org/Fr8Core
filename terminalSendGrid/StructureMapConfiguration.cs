using Fr8.Infrastructure.StructureMap;
using Fr8.Infrastructure.Utilities;
using SendGrid;
using StructureMap;
using terminalUtilities.Interfaces;
using terminalUtilities.SendGrid;

namespace terminalSendGrid
{
    public static class TerminalSendGridStructureMapBootstrapper
    {
        public class LiveMode : StructureMapBootStrapper.LiveMode
        {
            public LiveMode()
            {
                For<IEmailPackager>().Use<SendGridPackager>();
                For<ITransport>().Use(c => TransportFactory.CreateWeb(c.GetInstance<IConfigRepository>()));
            }
        }

        public static void LiveConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<LiveMode>();
        }
    }
}
