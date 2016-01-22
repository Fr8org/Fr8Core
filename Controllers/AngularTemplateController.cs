using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HubWeb.Controllers
{
    public class AngularTemplateController : Controller
    {
#if RELEASE
        [OutputCache(Duration = 3600, VaryByParam = "nocache,template")]
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