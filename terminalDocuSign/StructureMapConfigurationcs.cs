using Hub.StructureMap;
using StructureMap;
using StructureMap.Configuration.DSL;
using terminalDocuSign.Interfaces;
using terminalDocuSign.Services;
using terminalDocuSign.Services.New_Api;

namespace terminalDocuSign
{
    public class TerminalDocusignStructureMapBootstrapper
    {
        public class LiveMode : Registry
        {
            public LiveMode()
            {
                For<IDocuSignFolders>().Use<DocuSignFoldersWrapper>().Singleton();
                For<IDocuSignPlan>().Use<DocuSignPlan>().Singleton();
                For<IDocuSignManager>().Use<DocuSignManager>().Singleton();
                For<IDocuSignConnect>().Use<DocuSignConnect>().Singleton();
                For<IEvent>().Use<Event>().Singleton();
            }
        }

        public static void LiveConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<LiveMode>();
        }
    }
}