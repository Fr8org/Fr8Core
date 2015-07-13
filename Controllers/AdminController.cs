using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Daemons;
using Core.ExternalServices;
using Core.Managers;
using Web.ViewModels;

namespace Web.Controllers
{
    //[KwasantAuthorize(Roles = "Booker")]
    public class AdminController : Controller
    {
        //
        // GET: /Admin/
        public ActionResult Index()
        {
            Console.WriteLine("in Admin Index");
            return View();
        }

        public ActionResult Dashboard()
        {
            return View("Index");
        }
    }
}