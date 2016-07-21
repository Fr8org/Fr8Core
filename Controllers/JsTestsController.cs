using System.Web.Mvc;

namespace HubWeb.Controllers
{
    public class JsTestsController : Controller
    {
        // GET: JsTests
        public ActionResult Unit()
        {
            ViewBag.TestType = "Unit";
            return View("tests");
        }

        public ActionResult Integration()
        {
            ViewBag.TestType = "integration";
            return View("tests");
        }

        public ActionResult Endpoints()
        {
            ViewBag.TestType = "endpoints";
            return View("endpoints");
        }
    }
}