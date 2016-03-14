using Hub.StructureMap;
using Moq;
using StructureMap;
using StructureMap.Configuration.DSL;
using terminalDocuSign.Interfaces;
using terminalDocuSign.Services;

namespace terminalDocuSign.Infrastructure.StructureMap
{
    public class TerminalDocuSignMapBootstrapper
	{
		public static void ConfigureDependencies(StructureMapBootStrapper.DependencyType type)
		{


			switch (type)
			{
				case StructureMapBootStrapper.DependencyType.TEST:
					ObjectFactory.Configure(x => x.AddRegistry<TestMode>()); // No test mode yet
					break;
				case StructureMapBootStrapper.DependencyType.LIVE:
					ObjectFactory.Configure(x => x.AddRegistry<LiveMode>());
					break;
			}
		}

		public class LiveMode : Registry
		{
			public LiveMode()
			{
			    //For<IDocuSignFolder>().Use<DocuSignFolder>();
				//For<IDocuSignEnvelope>().Use<DocuSignEnvelope>();
				//For<IDocuSignTemplate>().Use<DocuSignTemplate>();
			    For<IDocuSignRoute>().Use<DocuSignRoute>();
                //For<IDocuSignFolder>().Use<DocuSignFolder>();
            }
		}

		public class TestMode : Registry
		{
			public TestMode()
			{
                //For<IDocuSignFolder>().Use<DocuSignFolder>();
				//For<IDocuSignEnvelope>().Use<DocuSignEnvelope>();
				//For<IDocuSignTemplate>().Use<DocuSignTemplate>();
                For<IDocuSignRoute>().Use(new Mock<DocuSignRoute>(MockBehavior.Default).Object);
                //For<IDocuSignFolder>().Use<DocuSignFolder>();
			}
		}

	}
}
