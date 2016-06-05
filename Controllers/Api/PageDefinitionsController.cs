using System;
using System.Collections.Generic;
using System.Web.Http;
using AutoMapper;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Fr8Data.DataTransferObjects;
using StructureMap;

namespace HubWeb.Controllers.Api
{
    public class PageDefinitionsController : ApiController
    {
        public static IList<PageDefinitionDO> PageDefinitions = new List<PageDefinitionDO>();

        private readonly ISecurityServices _securityServices;

        public PageDefinitionsController()
        {
            _securityServices = ObjectFactory.GetInstance<ISecurityServices>();
        }

        public IEnumerable<PageDefinitionDTO> Get()
        {
            return Mapper.Map<IList<PageDefinitionDTO>>(PageDefinitions);
        }

        public PageDefinitionDTO Get(int id)
        {
            return new PageDefinitionDTO();
        }

        public void Post([FromBody]PageDefinitionDTO pageDefinitionDTO)
        {
            var pageDefinition = Mapper.Map<PageDefinitionDO>(pageDefinitionDTO);
            //TODO: move to service
            if (pageDefinition.CreateDate == DateTimeOffset.MinValue)
            {
                pageDefinition.CreateDate = DateTimeOffset.Now;
            }
            pageDefinition.LastUpdated = DateTimeOffset.Now;
            pageDefinition.Author = "atata";
            PageDefinitions.Add(pageDefinition);
        }

        public void Put(int id, [FromBody]string pageDefinitionDTO)
        {
        }

        public void Delete(int id)
        {
        }
    }
}
