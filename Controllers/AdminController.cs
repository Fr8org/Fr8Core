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
    [Authorize]
    public class AdminController : Controller
    {
        //
        // GET: /Admin/
        public ActionResult Index()
        {
            return View();
        }
    }
}