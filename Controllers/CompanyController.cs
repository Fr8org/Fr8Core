using System.Web.Mvc;

namespace HubWeb.Controllers
{
    public class CompanyController : Controller
    {
        public ActionResult Jobs()
        {
            return View();
        }
    }
}