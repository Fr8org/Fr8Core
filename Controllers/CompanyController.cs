using System.Web.Mvc;

namespace HubWeb.Controllers
{
    public class CompanyController : Controller
    {
        // GET: Company
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
    }
}