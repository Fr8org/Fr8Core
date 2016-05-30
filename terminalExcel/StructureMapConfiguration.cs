using StructureMap;
using StructureMap.Configuration.DSL;
using Fr8Data.Managers;

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
