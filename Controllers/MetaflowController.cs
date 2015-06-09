
using System.Collections.Generic;
using System.Web.Mvc;
using KwasantCore.Managers.APIManagers.Packagers.Docusign;

namespace KwasantWeb.Controllers
{
    public class MetaflowController : Controller
    {
        private DocusignPackager _API;

        public MetaflowController()
        {
            _API = new DocusignPackager();
        }

        // GET: Envelope
        public ActionResult Index()
        {
            var query_params = new Dictionary<string, string>();
            query_params["from_date"] = "2015-05-11T07:00:00.0000000Z";
            var foo = _API.GetEnvelope(query_params);
            return View();
        }
    }
}