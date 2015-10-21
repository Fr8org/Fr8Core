using System;
using System.Web.Http;
using Core.Interfaces;
using Core.Services;
using StructureMap.Configuration.DSL;
using StructureMap;
using Core.StructureMap;
using terminalSendGrid.Infrastructure;
using terminalSendGrid.Services;
using SendGrid;
using Utilities;

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
