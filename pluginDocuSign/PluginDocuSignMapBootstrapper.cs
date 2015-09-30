using System;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Owin;
using StructureMap.Configuration.DSL;
using pluginDocuSign.Infrastructure;
using Core.Interfaces;
using Core.Services;
using StructureMap;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using DocuSign.Integrations.Client;
using pluginDocuSign.Interfaces;
using pluginDocuSign.Services;

namespace pluginDocuSign
{
	public class PluginDocuSignMapBootstrapper
	{
		public enum DependencyType
		{
			TEST = 0,
			LIVE = 1
		}

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
				For<IDocuSignRecipient>().Use<DocuSignRecipient>();
			}
		}

		public class TestMode : Registry
		{
			public TestMode()
			{
				For<IDocuSignEnvelope>().Use<DocuSignEnvelope>();
				For<IDocuSignTemplate>().Use<DocuSignTemplate>();
				For<IDocuSignRecipient>().Use<DocuSignRecipient>();
			}
		}

	}
}
