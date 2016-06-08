using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AutoMapper;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using fr8.Data.DataTransferObjects;
using Hub.Interfaces;
using StructureMap;

namespace HubWeb.Controllers.Api
{
    public class PageDefinitionsController : ApiController
    {
        private readonly IPageDefinition _pageDefinition;

        public PageDefinitionsController()
        {
            _pageDefinition = ObjectFactory.GetInstance<IPageDefinition>();
        }

        public IEnumerable<PageDefinitionDTO> Get()
        {
            var pageDefinitions = _pageDefinition.GetAll();
            return Mapper.Map<IList<PageDefinitionDTO>>(pageDefinitions);
        }

        public PageDefinitionDTO Get(int id)
        {
            var pageDefinition = _pageDefinition.Get(id);
            return Mapper.Map<PageDefinitionDTO>(pageDefinition);
        }

        public void Post([FromBody]PageDefinitionDTO pageDefinitionDTO)
        {
            var pageDefinition = Mapper.Map<PageDefinitionDO>(pageDefinitionDTO);
            _pageDefinition.CreateOrUpdate(pageDefinition);
        }

        public void Delete(int id)
        {
            _pageDefinition.Delete(id);
        }
    }
}
