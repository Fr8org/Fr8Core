using System.Web.Mvc;
using PlanDirectory.Infrastructure;

namespace PlanDirectory.Controllers
{
    [PlanDirectoryAuthorize]
    public class AngularTemplateController : Controller
    {
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