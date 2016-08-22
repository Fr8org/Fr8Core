using Hub.StructureMap;
using Moq;
using StructureMap;
using StructureMap.Configuration.DSL;
using terminalDocuSign.Interfaces;
using terminalDocuSign.Services;
using terminalDocuSign.Services.New_Api;

namespace terminalDocuSign.Infrastructure.StructureMap
{
    public class TerminalDocuSignMapBootstrapper
	{
		public static void ConfigureDependencies(StructureMapBootStrapper.DependencyType type)
		{
			switch (type)
			{
				case StructureMapBootStrapper.DependencyType.TEST:
					ObjectFactory.Configure(x => x.AddRegistry<TestMode>());
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
			    For<IDocuSignPlan>().Use<DocuSignPlan>();
			    For<IDocuSignManager>().Use<DocuSignManager>();
			    For<IDocuSignFolders>().Use<DocuSignFoldersWrapper>();
			}
		}

		public class TestMode : Registry
		{
			public TestMode()
			{
			    var docuSignPlanMock = new Mock<IDocuSignPlan>();
			    For<Mock<IDocuSignPlan>>().Use(docuSignPlanMock);
			    For<IDocuSignPlan>().Use(docuSignPlanMock.Object);
			    var docuSignManagerMock = new Mock<IDocuSignManager>();
                For<Mock<IDocuSignManager>>().Use(docuSignManagerMock);
                For<IDocuSignManager>().Use(docuSignManagerMock.Object);
                var docuSignFoldersMock = new Mock<IDocuSignFolders>();
                For<Mock<IDocuSignFolders>>().Use(docuSignFoldersMock);
                For<IDocuSignFolders>().Use(docuSignFoldersMock.Object);
            }
		}

	}
}
