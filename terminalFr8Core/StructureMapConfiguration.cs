using Hub.StructureMap;
using SendGrid;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using terminalUtilities.SendGrid;
using terminalUtilities.Twilio;
using Utilities;

namespace terminalFr8Core
{
    public class Fr8CoreStructureMapConfiguration
    {
        public enum DependencyType
        {
            TEST = 0,
            LIVE = 1
        }

        public static void ConfigureDependencies(DependencyType type)
        {
            switch (type)
            {
                case DependencyType.TEST:
                    ObjectFactory.Initialize(x => x.AddRegistry<LiveMode>()); // No test mode yet
                    break;
                case DependencyType.LIVE:
                    ObjectFactory.Initialize(x => x.AddRegistry<LiveMode>());
                    break;
            }
        }

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