using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Data.Infrastructure.AutoMapper;
using Hub.StructureMap;
using Microsoft.Owin;
using Owin;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using DependencyType = Hub.StructureMap.StructureMapBootStrapper.DependencyType;

[assembly: OwinStartup(typeof(terminalQuickBooks.Startup))]

namespace terminalQuickBooks
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app)
        {
            WebApiConfig.Register(new HttpConfiguration());
                StartHosting("terminal_QuickBooks");
        }
    }
}
