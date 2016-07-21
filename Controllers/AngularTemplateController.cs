using System.Web.Mvc;
using HubWeb.Infrastructure_HubWeb;

namespace HubWeb.Controllers
{
    public class AngularTemplateController : Controller
    {
#if RELEASE
        // [OutputCache(Duration = 3600, VaryByParam = "nocache,template")]
        [AngularTemplateCache]
#endif
        public ActionResult Markup(string template)
        {
            try
            {
                return View(string.Format("~/Views/AngularTemplate/{0}.cshtml", template));
            }
            catch
            {
                return HttpNotFound();
            }
        }
    }
}