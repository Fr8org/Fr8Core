using Data.Interfaces;
using Data.Wrappers;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Web.Controllers
{
    [Authorize]
    public class DocuSignTemplateController : ApiController
    {
        IDocuSignTemplate _template;

        public DocuSignTemplateController()
        {
            _template = ObjectFactory.GetInstance<IDocuSignTemplate>();
        }

        public IHttpActionResult Get()
        {
            return Ok(_template.GetTemplates(null));
        }
    }
}
