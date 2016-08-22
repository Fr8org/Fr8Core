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
using HubWeb.ViewModels;

namespace HubWeb.Controllers
{
    public class WelcomeController : Controller
    {
        //
        // GET: /Dashboard/
        public ActionResult Index()
        {
            return View();
        }
    }
}