using StructureMap;
using StructureMap.Configuration.DSL;
using TerminalSqlUtilities;

namespace terminalAzure
{
    public class TerminalAzureSqlServerStructureMapRegistries
    {
        public class LiveMode : Registry
        {
            public LiveMode()
            {
                For<IDbProvider>().Use<SqlClientDbProvider>();
                
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
