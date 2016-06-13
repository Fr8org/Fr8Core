using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.StructureMap;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Infrastructure;
using Fr8.TerminalBase.Interfaces;
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
        private readonly TerminalDTO _terminal;

        public IContainer Container => _container;
        public IActivityStore ActivityStore => _activityStore;

        protected BaseConfiguration(TerminalDTO terminal)
        {
            if (terminal == null)
                throw new ArgumentNullException(nameof(terminal));

            _terminal = terminal;
        }

        protected virtual void ConfigureProject(bool selfHost, Action<ConfigurationExpression> terminalStructureMapRegistryConfigExpression)
        {
            _container = new Container(StructureMapBootStrapper.LiveConfiguration);
            _activityStore = new ActivityStore(_terminal, _container);

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

        protected virtual void StartHosting()
        {

        }

        public abstract ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver);
        

        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            var childContainer = _container.CreateChildContainer();
            Expression<Func<IContext, IHubCommunicator>> hubCommunicatorFactoryExpression;
            
            if (request.Headers.Contains("Fr8HubCallBackUrl") && request.Headers.Contains("Fr8HubCallbackSecret"))
            {
                var apiUrl = request.Headers.GetValues("Fr8HubCallBackUrl").First();
                var secret = request.Headers.GetValues("Fr8HubCallbackSecret").First();
                var terminalId = CloudConfigurationManager.GetSetting("TerminalId") ?? ConfigurationManager.AppSettings[_activityStore.Terminal.Name + "TerminalId"];

                hubCommunicatorFactoryExpression = c => new DefaultHubCommunicator(c.GetInstance<IRestfulServiceClient>(), c.GetInstance<IHMACService>(), apiUrl, terminalId, secret);
            }
            else
            {
                hubCommunicatorFactoryExpression = c => new DealyedhubCommunicator(c.GetInstance<IHubEventReporter>());
            }
            
            childContainer.Configure(x => x.For<IHubCommunicator>().Use(hubCommunicatorFactoryExpression));
            childContainer.Configure(x => x.For<IContainer>().Use(childContainer));

            request.RegisterForDispose(childContainer);

            return childContainer.GetInstance(controllerType) as IHttpController;
        }
    }
}
