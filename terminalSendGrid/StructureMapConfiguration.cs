using Hub.StructureMap;
using SendGrid;
using StructureMap;
using Utilities;
using terminalSendGrid.Infrastructure;
using terminalSendGrid.Services;

namespace terminalSendGrid
{
    public static class TerminalSendGridStructureMapBootstrapper
    {
        public static void SendGridConfigureDependencies(this IContainer container, StructureMapBootStrapper.DependencyType type)
        {
            switch (type)
            {
                case StructureMapBootStrapper.DependencyType.TEST:
                case StructureMapBootStrapper.DependencyType.LIVE:
                    ObjectFactory.Configure(cfg => cfg.For<IEmailPackager>().Use<SendGridPackager>());
                    ObjectFactory.Configure(cfg => cfg.For<ITransport>().Use(c => TransportFactory.CreateWeb(c.GetInstance<IConfigRepository>())));
                    break;
            }
        }
    }
}
