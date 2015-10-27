using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using Hub.Managers;
using Web.ViewModels;

namespace Web.Controllers
{
    [DockyardAuthorize]
    public class DashboardController : Controller
    {
        //
        // GET: /Dashboard/
        public ActionResult Index(int id = 0)
        {
            return View();
        }

        //
        // GET: /Dashboard/Sandbox
        public ActionResult Sandbox(int id = 0)
        {
            return View("../Sandbox/Index");
        }
    }
}