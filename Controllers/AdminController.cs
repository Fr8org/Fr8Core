using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Daemons;
using KwasantCore.ExternalServices;
using KwasantCore.Managers;
using KwasantWeb.ViewModels;

namespace KwasantWeb.Controllers
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