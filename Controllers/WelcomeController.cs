using System.Web.Mvc;

namespace HubWeb.Controllers
{
    public class WelcomeController : Controller
    {
        //
        // GET: /Dashboard/
        public ActionResult Index()
        {
            return View();
        }
    }
}