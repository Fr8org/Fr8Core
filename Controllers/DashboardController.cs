using System.Web.Mvc;
using Hub.Managers;

namespace HubWeb.Controllers
{
    [DockyardAuthorize]
    public class DashboardController : Controller
    {
        //
        // GET: /Dashboard/
#if RELEASE
        [OutputCache(Duration = 43200, Location = System.Web.UI.OutputCacheLocation.Client)]
#endif
        public ActionResult Index(int id = 0)
        {
            return View();
        }

        //
        // GET: /Dashboard/Sandbox
        public ActionResult Sandbox(int id = 0)
        {
            return View("../Sandbox/Index");
        }
    }
}