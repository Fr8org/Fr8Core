using System;
using System.Web.Mvc;
using Hub.Managers;

namespace PlanDirectory.Controllers
{
    [DockyardAuthorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
