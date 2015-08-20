using System.Collections.Generic;
using System.Web.Http;
using Data.Interfaces;
using StructureMap;

namespace Web.Controllers
{
    public class FieldsController :ApiController
    {
        private ITemplate _template;
        public FieldsController()
        {
            _template = ObjectFactory.GetInstance<ITemplate>();
        }

        public IHttpActionResult Get(string id)
        {
            return Ok(_template.GetMappableSourceFields(id));
        }
    }
}