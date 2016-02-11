using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Hub.Services;
using HubWeb;
using HubWeb.Controllers;
using HubWeb.Controllers.Api;
using Microsoft.Owin.Hosting;
using Owin;
using System.Web.Http.Controllers;
using System.Net.Http;
using System.Linq;

namespace HubWeb
{
    public class SelfHostFactory
    {
        public class SelfHostStartup
        {
            public void Configuration(IAppBuilder app)
            {
                HttpConfiguration config = new HttpConfiguration();
                WebApiConfig.Register(config);
                config.Services.Replace(
                    typeof(IHttpControllerSelector),
                    new NamespaceHttpControllerSelector(config, "HubWeb.Controllers"));
                app.UseWebApi(config);
                var startup = new Startup();
                startup.Configuration(app, true);
                new MvcApplication().Init(true);
            }
        }
        public static IDisposable CreateServer(string url)
        {
            return WebApp.Start<SelfHostStartup>(url: url);
        }

        public ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {   
            return new Type[] {
                    typeof(ActionListController),
                    typeof(ActionsController),
                    typeof(AlarmsController),
                    typeof(AuthenticationController),
                    typeof(ConfigurationController),
                    typeof(ContainersController),
                    typeof(CriteriaController),
                    typeof(EventController),
                    typeof(FieldController),
                    typeof(FilesController),
                    typeof(ManageAuthTokenController),
                    typeof(ManifestsController),
                    typeof(ReportController),
                    typeof(RouteNodesController),
                    typeof(RoutesController),
                    typeof(ProcessNodeTemplateController),
                    typeof(TerminalsController),
                    typeof(UserController),
                    typeof(WebServicesController)
                };
        }
    }

    // Copyright Umbarco CMS 
    // Source: https://github.com/umbraco/Umbraco-CMS/blob/7.1.5/src/Umbraco.Web/WebApi/NamespaceHttpControllerSelector.cs
    public class NamespaceHttpControllerSelector : DefaultHttpControllerSelector
    {
        private const string ControllerKey = "controller";
        private readonly HttpConfiguration _configuration;
        private readonly Lazy<IEnumerable<Type>> _duplicateControllerTypes;
        private string _ns;

        public NamespaceHttpControllerSelector(HttpConfiguration configuration, string ns) : base(configuration)
        {
            _configuration = configuration;
            _duplicateControllerTypes = new Lazy<IEnumerable<Type>>(GetDuplicateControllerTypes);
            _ns = ns;
        }

        public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            var routeData = request.GetRouteData();
            if (routeData == null || routeData.Route == null)
                return base.SelectController(request);

            // Look up controller in route data
            object controllerName;
            routeData.Values.TryGetValue(ControllerKey, out controllerName);
            var controllerNameAsString = controllerName as string;
            if (controllerNameAsString == null)
                return base.SelectController(request);

            //get the currently cached default controllers - this will not contain duplicate controllers found so if
            // this controller is found in the underlying cache we don't need to do anything
            var map = base.GetControllerMapping();
            if (map.ContainsKey(controllerNameAsString))
                return base.SelectController(request);

            //the cache does not contain this controller because it's most likely a duplicate, 
            // so we need to sort this out ourselves and we can only do that if the namespace token
            // is formatted correctly.
            if (_ns == null)
                return base.SelectController(request);

            //see if this is in our cache
            var found = _duplicateControllerTypes.Value
                .Where(x => string.Equals(x.Name, controllerNameAsString + ControllerSuffix, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault(x => _ns == x.Namespace);

            if (found == null)
                return base.SelectController(request);

            return new HttpControllerDescriptor(_configuration, controllerNameAsString, found);
        }

        private IEnumerable<Type> GetDuplicateControllerTypes()
        {
            var assembliesResolver = _configuration.Services.GetAssembliesResolver();
            var controllersResolver = _configuration.Services.GetHttpControllerTypeResolver();
            var controllerTypes = controllersResolver.GetControllerTypes(assembliesResolver);

            //we have all controller types, so just store the ones with duplicate class names - we don't
            // want to cache too much and the underlying selector caches everything else

            var duplicates = controllerTypes.GroupBy(x => x.Name)
                .Where(x => x.Count() > 1)
                .SelectMany(x => x)
                .ToArray();

            return duplicates;
        }

    }
}
