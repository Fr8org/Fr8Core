using System.Web.Mvc;

namespace HubWeb.Controllers
{
    
    public class PlanDirectoryController : Controller
    {

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }       
    }
}
