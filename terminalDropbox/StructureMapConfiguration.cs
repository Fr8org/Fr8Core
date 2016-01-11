using StructureMap;
using Hub.Interfaces;
using Hub.Services;
using Hub.StructureMap;
using Hub.Managers;
using terminalDropbox.Interfaces;
using terminalDropbox.Services;

namespace terminalDropbox
{
    public class TerminalDropboxStructureMapBootstrapper
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
                For<IAction>().Use<Hub.Services.Action>();
                For<ITerminal>().Use<Terminal>();
                For<ICrateManager>().Use<CrateManager>();
                For<IRouteNode>().Use<RouteNode>();
                For<IDropboxService>().Use<DropboxService>();
            }
        }

        public static void LiveConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<LiveMode>();
        }

    }
}
