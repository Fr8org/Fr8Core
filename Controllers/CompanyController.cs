using System.Web.Mvc;

namespace HubWeb.Controllers
{
    public class CompanyController : Controller
    {
        public ActionResult Jobs()
        {
            return View();
        }
        // GET: Company
        [HttpGet]
        public ActionResult Index(string id)
        {
            ViewBag.SectionId = id;
            return View();
        }
        //[HttpPost]
        //public ActionResult Index(string SectionId)
        //{
            
        //    return View();
        //}
    }
}