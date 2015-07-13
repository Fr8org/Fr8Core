
using System.Collections.Generic;
using System.Web.Mvc;
using Data.Entities;
using Core.Managers.APIManagers.Packagers.Docusign;
using Core.Services;

namespace Web.Controllers
{
    public class MetaflowController : Controller
    {
        private DocusignPackager _API;
        private Metaflow _flow;
        public MetaflowController()
        {
            _API = new DocusignPackager();
            _flow = new Metaflow();
        }

        // GET: Envelope
        public ActionResult Create()
        {
            _flow.Create();
            return View();
        }
    }
}