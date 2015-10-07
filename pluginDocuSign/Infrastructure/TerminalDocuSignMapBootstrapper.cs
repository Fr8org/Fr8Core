using StructureMap;
using StructureMap.Configuration.DSL;
using terminal_DocuSign.Interfaces;
using terminal_DocuSign.Services;
using DependencyType = Core.StructureMap.StructureMapBootStrapper.DependencyType;

namespace terminal_DocuSign.Infrastructure.StructureMap
{
	public class TerminalDocuSignMapBootstrapper
	{
		public static void ConfigureDependencies(DependencyType type)
		{
			switch (type)
			{
				case DependencyType.TEST:
					ObjectFactory.Configure(x => x.AddRegistry<TestMode>()); // No test mode yet
					break;
				case DependencyType.LIVE:
					ObjectFactory.Configure(x => x.AddRegistry<LiveMode>());
					break;
			}
		}

		public class LiveMode : Registry
		{
			public LiveMode()
			{
				For<IDocuSignEnvelope>().Use<DocuSignEnvelope>();
				For<IDocuSignTemplate>().Use<DocuSignTemplate>();
			}
		}

		public class TestMode : Registry
		{
			public TestMode()
			{
				For<IDocuSignEnvelope>().Use<DocuSignEnvelope>();
				For<IDocuSignTemplate>().Use<DocuSignTemplate>();
			}
		}

	}
}
