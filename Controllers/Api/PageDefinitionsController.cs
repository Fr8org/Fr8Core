using System.Collections.Generic;
using System.Web.Http;
using AutoMapper;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.States;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Hub.Interfaces;
using Hub.Managers;
using StructureMap;

namespace HubWeb.Controllers.Api
{
    public class PageDefinitionsController : ApiController
    {
        private readonly IPageDefinition _pageDefinition;
        private readonly ISecurityServices _securityServices;

        public PageDefinitionsController()
        {
            _pageDefinition = ObjectFactory.GetInstance<IPageDefinition>();
            _securityServices = ObjectFactory.GetInstance<ISecurityServices>();
        }

        public IEnumerable<PageDefinitionDTO> Get()
        {
            var pageDefinitions = _pageDefinition.GetAll();
            return Mapper.Map<IList<PageDefinitionDTO>>(pageDefinitions);
        }

        [DockyardAuthorize(Roles = Roles.Admin)]
        public PageDefinitionDTO Get(int id)
        {
            var pageDefinition = _pageDefinition.Get(id);
            return Mapper.Map<PageDefinitionDTO>(pageDefinition);
        }

        public void Post([FromBody]PageDefinitionDTO pageDefinitionDTO)
        {
            if (_securityServices.UserHasPermission(PermissionType.EditPageDefinitions, nameof(PageDefinitionDO)))
            {
                var pageDefinition = Mapper.Map<PageDefinitionDO>(pageDefinitionDTO);
                _pageDefinition.CreateOrUpdate(pageDefinition);
            }
        }

        public void Delete(int id)
        {
            _pageDefinition.Delete(id);
        }
    }
}
