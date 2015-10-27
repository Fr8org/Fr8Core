using System;
using System.Web.Http;
using SendGrid;
using StructureMap;
using StructureMap.Configuration.DSL;
using Hub.Interfaces;
using Hub.Services;
using Hub.StructureMap;
using Utilities;
using terminalSendGrid.Infrastructure;
using terminalSendGrid.Services;

namespace terminalSendGrid
{
    public class PluginSendGridStructureMapBootstrapper
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
                case DependencyType.LIVE:
                    ObjectFactory.Configure(cfg => cfg.For<IEmailPackager>().Use<SendGridPackager>());
                    ObjectFactory.Configure(cfg => cfg.For<ITransport>().Use(c => TransportFactory.CreateWeb(c.GetInstance<IConfigRepository>())));
                    break;
            }
        }
    }
}
