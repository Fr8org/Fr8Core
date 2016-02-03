using System.Linq;
using System.Web.Mvc;

namespace HubWeb.Infrastructure
{
    public class AngularTemplateCacheAttribute : OutputCacheAttribute
    {
        private static readonly string[] ExcludedTemplates = new[]
        {
            "Header"
        };

        public AngularTemplateCacheAttribute()
        {
            Duration = 3600;
            VaryByParam = "nocache,template";
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