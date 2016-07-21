using Fr8.Infrastructure.Utilities;
using Hub.StructureMap;
using SendGrid;
using StructureMap;
using terminalUtilities.Interfaces;
using terminalUtilities.SendGrid;
using terminalUtilities.Twilio;

namespace terminalFr8Core
{
    public class Fr8CoreStructureMapConfiguration
    {
        public class LiveMode : StructureMapBootStrapper.LiveMode
        {
            public LiveMode()
            {
                For<IEmailPackager>().Use<SendGridPackager>();
                For<ITransport>().Use(c => TransportFactory.CreateWeb(c.GetInstance<IConfigRepository>()));
                For<ITwilioService>().Use<TwilioService>();
            }
        }

        public static void LiveConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<LiveMode>();
        }
    }
}