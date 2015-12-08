using Data.Infrastructure.AutoMapper;
using Hub.StructureMap;
using Microsoft.Owin;
using Owin;
using TerminalBase.BaseClasses;
using terminalGoogle;
using TerminalBase.Infrastructure;
using DependencyType = Hub.StructureMap.StructureMapBootStrapper.DependencyType;

[assembly: OwinStartup(typeof(terminalGoogle.Startup))]

namespace terminalGoogle
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app)
        {
            
            DataAutoMapperBootStrapper.ConfigureAutoMapper();
            StructureMapBootStrapper.ConfigureDependencies(DependencyType.LIVE).ConfigureGoogleDependencies(DependencyType.LIVE);
            TerminalBootstrapper.ConfigureLive();
            StartHosting("terminal_Google");
        }
    }
}
