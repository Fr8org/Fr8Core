using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Fr8Infrastructure.StructureMap;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StructureMap;
using TerminalBase.Infrastructure;

namespace TerminalBase.BaseClasses
{
    public abstract class BaseConfiguration : IHttpControllerTypeResolver
    {
        protected HttpConfiguration _configuration = new HttpConfiguration();

        protected virtual void ConfigureProject(bool selfHost, Action<ConfigurationExpression> terminalStructureMapRegistryConfigExpression)
        {
            ObjectFactory.Initialize();
            ObjectFactory.Configure(StructureMapBootStrapper.LiveConfiguration);
            TerminalBootstrapper.ConfigureLive();
            AutoMapperBootstrapper.ConfigureAutoMapper();

            if (terminalStructureMapRegistryConfigExpression != null)
            {
                ObjectFactory.Configure(terminalStructureMapRegistryConfigExpression);
            }

            if (selfHost)
            {
                // Web API routes
                _configuration.Services.Replace(typeof(IHttpControllerTypeResolver), this);
            }


            RegisterActivities();
        }

        protected abstract void RegisterActivities();

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

        protected virtual void StartHosting(string terminalName)
        {
            Task.Run(() =>
            {
                BaseTerminalController curController = new BaseTerminalController();
                curController.AfterStartup(terminalName);
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
