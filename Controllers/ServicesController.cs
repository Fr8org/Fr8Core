using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HubWeb.Controllers
{
    public class ServicesController : Controller
    {
        public ActionResult DocuSign()
        {
            return View();
        }

        public ActionResult HowItWorks()
        {
            return View();
        }

        public ActionResult Salesforce()
        {
            return View();
        }

        public ActionResult GoogleApps()
        {
            return View();
        }
    }
}