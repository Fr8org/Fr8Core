using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Data.Entities;
using Web.ViewModels;
using AutoMapper;
using StructureMap;
using Data.Interfaces;
using Data.States;
using Core.Managers;
using Core.Exceptions;

namespace Web.Controllers
{
    [Authorize]
    public class ProcessTemplateController : Controller
    {
        // GET: ProcessTemplate
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ProcessBuilderExample()
        {
            return View();
        }
    }
}
