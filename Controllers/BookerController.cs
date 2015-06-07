using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KwasantWeb.Controllers
{
    public class BookerController : Controller
    {
        //
        // GET: /Booker/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Dashboard()
        {
            return View("Index");
        }
	}
}