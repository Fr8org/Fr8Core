using System.Web.Mvc;
using Fr8.Infrastructure.Utilities;
using Microsoft.Ajax.Utilities;

namespace HubWeb.Controllers
{
    public class WelcomeController : Controller
    {
        // GET: /Dashboard/
        public ActionResult Index()
        {
            if (TempData.Keys.Contains("guestUserId"))
                ViewBag.AnalyticsUserId = TempData["guestUserId"].ToString();
            if (TempData.Keys.Contains("mode"))
                ViewBag.Mode = TempData["mode"].ToString();
            return View();
        }
    }
}