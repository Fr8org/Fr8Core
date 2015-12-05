using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Daemons;
using Hub.ExternalServices;
using Hub.Managers;
using HubWeb.ViewModels;

namespace HubWeb.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        //
        // GET: /Admin/
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult RouteBuilderExample()
        {
            return View();
        }
    }
}