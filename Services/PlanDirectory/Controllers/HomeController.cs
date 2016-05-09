using System;
using System.Web.Mvc;

namespace PlanDirectory.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}