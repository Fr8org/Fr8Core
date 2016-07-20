using Fr8.Infrastructure.StructureMap;
using StructureMap;
using terminalDropbox.Interfaces;
using terminalDropbox.Services;

namespace terminalDropbox
{
    public class TerminalDropboxStructureMapBootstrapper
    {
        public class LiveMode : StructureMapBootStrapper.LiveMode
        {
            public LiveMode()
            {
                For<IDropboxService>().Use<DropboxService>();
            }
        }

        public static void LiveConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<LiveMode>();
        }

    }
}
