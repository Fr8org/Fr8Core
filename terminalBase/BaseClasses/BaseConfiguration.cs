using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StructureMap;
using Data.Infrastructure.AutoMapper;
using Hub.StructureMap;

namespace TerminalBase.BaseClasses
{
    public class BaseConfiguration : IHttpControllerTypeResolver
    {
        protected HttpConfiguration _configuration = new HttpConfiguration();

        protected virtual void ConfigureProject(bool selfHost, Action<ConfigurationExpression> pluginStructureMapRegistryConfigExpression)
        {
            if (!selfHost)
            {
                ObjectFactory.Initialize();
                ObjectFactory.Configure(StructureMapBootStrapper.LiveConfiguration);
            }
            ObjectFactory.Configure(pluginStructureMapRegistryConfigExpression);

            if (selfHost)
            {
                // Web API routes
                _configuration.Services.Replace(typeof(IHttpControllerTypeResolver), this);
            }

            DataAutoMapperBootStrapper.ConfigureAutoMapper();
        }

        protected virtual void ConfigureFormatters()
        {
            // Configure formatters
            // Enable camelCasing in JSON responses
            var formatters = _configuration.Formatters;
            var jsonFormatter = formatters.JsonFormatter;
            var settings = jsonFormatter.SerializerSettings;
            settings.Formatting = Formatting.Indented;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }

        protected virtual void StartHosting(string pluginName)
        {
            Task.Run(() =>
            {
                BasePluginController curController = new BasePluginController();
                curController.AfterStartup(pluginName);
            });
        }

        public virtual ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            // this method should be implemented by the child classes; hence virtual but since it 
            // has to implement IHttpControllerTypeResolver therefore the access is public
            throw new NotImplementedException();
        }
    }
}
