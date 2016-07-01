using System.Web.Mvc;

namespace HubWeb.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        //
        // GET: /Admin/
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult RouteBuilderExample()
        {
            return View();
        }
    }
}