using Fr8.Infrastructure.Data.Managers;
using StructureMap;
using StructureMap.Configuration.DSL;

namespace terminalExcel
{
    public class TerminalExcelStructureMapRegistries
    {
        public class LiveMode : Registry
        {
            public LiveMode()
            {
                For<ICrateManager>().Use<CrateManager>();
            }
        }

        public static void LiveConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<LiveMode>();
        }

        public static void TestConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<LiveMode>();
        }
    }
}
