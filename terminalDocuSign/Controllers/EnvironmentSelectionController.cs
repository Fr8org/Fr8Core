using System.Web.Mvc;

namespace terminalDocuSign.Controllers
{
    public class EnvironmentSelectionController : Controller
    {
        public ActionResult Index(string state)
        {
            return View((object)state);
        }
    }
}