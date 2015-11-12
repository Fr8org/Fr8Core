using System;
using System.Web.Http;
using DocuSign.Integrations.Client;
using Microsoft.Owin.Hosting;
using Owin;
using StructureMap;
using StructureMap.Configuration.DSL;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Hub.Interfaces;
using Hub.Services;
using terminalDocuSign.Infrastructure;
using terminalDocuSign.Interfaces;
using terminalDocuSign.Services;

using DependencyType = Hub.StructureMap.StructureMapBootStrapper.DependencyType;

namespace terminalDocuSign.Infrastructure.StructureMap
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
