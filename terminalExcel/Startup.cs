using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Core.StructureMap;
using Data.Infrastructure.AutoMapper;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;
using PluginBase;
using PluginBase.BaseClasses;
using StructureMap;

[assembly: OwinStartup("PluginExcelConfiguration", typeof(terminalExcel.StartupPluginExcel))]

namespace terminalExcel
{
    public class StartupPluginExcel : BaseConfiguration
    {
        public void Configuration(IAppBuilder app)
        {
            Configuration(app, false);
        }

        public void Configuration(IAppBuilder app, bool selfHost)
        {
            ConfigureProject(selfHost, PluginExcelStructureMapRegistries.LiveConfiguration);

            RoutesConfig.Register(_configuration);

            //if (selfHost)
            //{
            // Web API routes
            _configuration.Services.Replace(typeof(IHttpControllerTypeResolver), new PluginControllerTypeResolver());
            //}

            //DataAutoMapperBootStrapper.ConfigureAutoMapper();

            ConfigureFormatters();

            app.UseWebApi(_configuration);

            if (!selfHost)
            {
                StartHosting("plugin_excel");
            }
        }

        //public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        //{
        //    return new Type[] {
        //            typeof(Controllers.ActionController),
        //            typeof(Controllers.EventController),
        //            typeof(Controllers.PluginController)
        //        };
        //}

        public class PluginControllerTypeResolver : IHttpControllerTypeResolver
        {
            public ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                    typeof(Controllers.ActionController),
                    typeof(Controllers.EventController),
                    typeof(Controllers.PluginController)
                };
        }
        }
    }
}
