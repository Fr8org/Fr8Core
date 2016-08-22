using System.Linq;
using System.Web.Mvc;

namespace HubWeb.Infrastructure_HubWeb
{
    public class AngularTemplateCacheAttribute : OutputCacheAttribute
    {
        private static readonly string[] ExcludedTemplates = new[]
        {
            "Header"
        };

        public AngularTemplateCacheAttribute()
        {
            //cache 12 hours
            Duration = 43200;
            VaryByParam = "nocache,template";
            Location = System.Web.UI.OutputCacheLocation.Client;
        }

        private bool IsExcludedTemplate(ControllerContext filterContext)
        {
            bool excluded = ExcludedTemplates != null
                && ExcludedTemplates.Contains(filterContext.RouteData.Values["template"]);

            return excluded;
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (!IsExcludedTemplate(filterContext))
            {
                base.OnResultExecuting(filterContext);
            }
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (!IsExcludedTemplate(filterContext))
            {
                base.OnResultExecuted(filterContext);
            }
        }
    }
}