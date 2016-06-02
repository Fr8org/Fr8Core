using System.Collections.Generic;
using System.Web.Http;
using PlanDirectory.Interfaces;

namespace PlanDirectory.Controllers.Api
{
    public class PageDefinitionsController : ApiController
    {
        // GET: api/PageDefinitions
        public IEnumerable<PageDefinitionDTO> Get()
        {
            return new List<PageDefinitionDTO> { new PageDefinitionDTO() };
        }

        // GET: api/PageDefinitions/5
        public PageDefinitionDTO Get(int id)
        {
            return new PageDefinitionDTO();
        }

        // POST: api/PageDefinitions
        public void Post([FromBody]PageDefinitionDTO pageDefinitionDTO)
        {
        }

        // PUT: api/PageDefinitions/5
        public void Put(int id, [FromBody]string pageDefinitionDTO)
        {
        }

        // DELETE: api/PageDefinitions/5
        public void Delete(int id)
        {
        }
    }
}
