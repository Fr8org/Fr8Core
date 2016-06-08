using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace Hub.Infrastructure
{
    public class CustomSelector : DefaultHttpControllerSelector
    {
        private readonly HttpConfiguration _configuration;

        public CustomSelector(HttpConfiguration configuration) : base(configuration)
        {
            _configuration = configuration;
        }

        public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            var controllerName = base.GetControllerName(request);
            if (controllerName.Contains("_"))
            {
                IAssembliesResolver assembliesResolver = _configuration.Services.GetAssembliesResolver();
                IHttpControllerTypeResolver httpControllerTypeResolver = this._configuration.Services.GetHttpControllerTypeResolver();
                ICollection<Type> controllerTypes = httpControllerTypeResolver.GetControllerTypes(assembliesResolver);
                controllerName = controllerName.Replace("_", "");
                var matchedController =
                    controllerTypes.FirstOrDefault(i => i.Name.ToLower() == controllerName.ToLower() + "controller");

                return new HttpControllerDescriptor(_configuration, controllerName, matchedController);
            }

            return base.SelectController(request);
        }
    }
}
