using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.StructureMap;
using Fr8.TerminalBase.Infrastructure;
using Fr8.TerminalBase.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StructureMap;

namespace Fr8.TerminalBase.BaseClasses
{
    public abstract class BaseConfiguration : IHttpControllerTypeResolver, IHttpControllerActivator
    {
        protected HttpConfiguration _configuration = new HttpConfiguration();
        private IContainer _container;
        private IActivityStore _activityStore;

        public IContainer Container => _container;
        public IActivityStore ActivityStore => _activityStore;

        protected virtual void ConfigureProject(bool selfHost, Action<ConfigurationExpression> terminalStructureMapRegistryConfigExpression)
        {
            _container = new Container(StructureMapBootStrapper.LiveConfiguration);
            _activityStore = new ActivityStore(_container);

            _container.Configure(x => x.AddRegistry<TerminalBootstrapper.LiveMode>());
            _container.Configure(x => x.For<IActivityStore>().Use(_activityStore));

            AutoMapperBootstrapper.ConfigureAutoMapper();

            if (terminalStructureMapRegistryConfigExpression != null)
            {
                _container.Configure(terminalStructureMapRegistryConfigExpression);
            }

            if (selfHost)
            {
                // Web API routes
                _configuration.Services.Replace(typeof(IHttpControllerTypeResolver), this);
            }

            _configuration.Services.Replace(typeof(IHttpControllerActivator), this);

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
                BaseTerminalController curController = new BaseTerminalController(Container.GetInstance<IRestfulServiceClient>());
                curController.AfterStartup(terminalName);
            });
        }

        public virtual ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            // this method should be implemented by the child classes; hence virtual but since it 
            // has to implement IHttpControllerTypeResolver therefore the access is public
            throw new NotImplementedException();
        }

        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            return _container.GetInstance(controllerType) as IHttpController;
        }
    }
}
