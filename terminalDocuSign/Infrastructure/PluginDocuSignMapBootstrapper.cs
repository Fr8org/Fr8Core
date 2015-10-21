using System;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Owin;
using StructureMap.Configuration.DSL;
using terminalDocuSign.Infrastructure;
using Core.Interfaces;
using Core.Services;
using StructureMap;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using DocuSign.Integrations.Client;
using terminalDocuSign.Interfaces;
using terminalDocuSign.Services;

using DependencyType = Core.StructureMap.StructureMapBootStrapper.DependencyType;

namespace terminalDocuSign.Infrastructure.StructureMap
{
	public class PluginDocuSignMapBootstrapper
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
