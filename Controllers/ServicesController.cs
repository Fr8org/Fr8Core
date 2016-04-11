using Hub.Interfaces;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HubWeb.Controllers
{
    public class ServicesController : Controller
    {
        private readonly IActivity _activity = ObjectFactory.GetInstance<IActivity>();
        private List<string> getDocumentationSolutionList(string terminalName)
        {
            var solutionNameList = _activity.GetSolutionList(terminalName);
            return solutionNameList;
        }
        public ActionResult DocuSign()
        {
            List<string> solutionList = getDocumentationSolutionList("DocuSign");
            return View(solutionList);
        }

        public ActionResult HowItWorks()
        {
            return View();
        }

        public ActionResult Salesforce()
        {
            getDocumentationSolutionList("Salesforce");
            return View();
        }

        public ActionResult GoogleApps()
        {
            getDocumentationSolutionList("GoogleApps");
            return View();
        }
    }
}