using System.Collections.Generic;
using System.Web.Http;
using Fr8Data.DataTransferObjects;

namespace HubWeb.Controllers.Api
{
    public class PageDefinitionsController : ApiController
    {
        public IEnumerable<PageDefinitionDTO> Get()
        {
            return new List<PageDefinitionDTO> { new PageDefinitionDTO() };
        }
        
        public PageDefinitionDTO Get(int id)
        {
            return new PageDefinitionDTO();
        }
        
        public void Post([FromBody]PageDefinitionDTO pageDefinitionDTO)
        {
        }
        
        public void Put(int id, [FromBody]string pageDefinitionDTO)
        {
        }
        
        public void Delete(int id)
        {
        }
    }
}
