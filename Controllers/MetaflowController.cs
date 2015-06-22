
using System.Collections.Generic;
using System.Web.Mvc;
using Data.Entities;
using KwasantCore.Managers.APIManagers.Packagers.Docusign;
using KwasantCore.Services;

namespace KwasantWeb.Controllers
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