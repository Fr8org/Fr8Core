using System.Collections.Generic;
using System.Web.Http;
using Data.Interfaces;
using StructureMap;

namespace Web.Controllers
{
    public class TemplateController :ApiController
    {
        private ITemplate _template;
        public TemplateController()
        {
            _template = ObjectFactory.GetInstance<ITemplate>();
        }

        [Route("Fields")]
        [HttpGet]
        public IHttpActionResult GetFields(string templateId)
        {
            return Ok(_template.GetMappableSourceFields(templateId));
        }
    }
}