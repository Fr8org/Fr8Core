using System;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Owin;
using StructureMap;
using StructureMap.Configuration.DSL;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Hub.Interfaces;
using Hub.Services;
using terminalGoogle.Interfaces;
using terminalGoogle.Services;
using Hub.Managers;
using StructureMap;
using Hub.StructureMap;
using terminalGoogle.Services.Authorization;

namespace terminalGoogle
{
    public static class TerminalGoogleBootstrapper
	{
        public static void ConfigureGoogleDependencies(this StructureMap.IContainer container, StructureMapBootStrapper.DependencyType type)
        {
            switch (type)
            {
                case StructureMapBootStrapper.DependencyType.TEST:
                    container.Configure(ConfigureLive); // no test mode yet
                    break;

                case StructureMapBootStrapper.DependencyType.LIVE:
                    container.Configure(ConfigureLive);
                    break;
            }
        }

        /**********************************************************************************/

        public static void ConfigureLive(ConfigurationExpression configurationExpression)
        {
            configurationExpression.For<IGoogleIntegration>().Use<GoogleIntegration>();
            configurationExpression.For<IGoogleSheet>().Use<GoogleSheet>();
            configurationExpression.For<ICrateManager>().Use<CrateManager>();
        }

	}
}
