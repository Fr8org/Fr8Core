using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Dispatcher;

namespace PlanDirectory
{
    public class PlanDirectoryHttpControllerTypeResolver : IHttpControllerTypeResolver
    {
        public ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                    typeof(Controllers.Api.AuthenticationController),
                    typeof(Controllers.Api.PlanTemplatesController)
                };
        }
    }
}