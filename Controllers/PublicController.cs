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
    public class PublicController : Controller
    {
        public ActionResult Footer()
        {
            return View();
        }

        public ActionResult Header()
        {
            return View();
        }

        public ActionResult PageHead()
        {
            return View();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SignIn()
        {
            return View();
        }
    }
}