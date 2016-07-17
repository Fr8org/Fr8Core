using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using AutoMapper;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.States;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Hub.Interfaces;
using Hub.Managers;
using StructureMap;
using System.Web.Http.Description;
using Swashbuckle.Swagger.Annotations;

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
        /// <summary>
        /// Retrieves a collection of all page definitions
        /// </summary>
        /// <response code="200">Collection of page definitions</response>
        [DockyardAuthorize(Roles = Roles.Admin)]
        [ResponseType(typeof(IList<PageDefinitionDTO>))]
        public IEnumerable<PageDefinitionDTO> Get()
        {
            var pageDefinitions = _pageDefinition.GetAll();
            return Mapper.Map<IList<PageDefinitionDTO>>(pageDefinitions);
        }
        /// <summary>
        /// Retrieves a page definition with specified Id
        /// </summary>
        /// <param name="id">Id of page definition</param>
        /// <response code="200">Page definition with specified Id</response>
        [DockyardAuthorize(Roles = Roles.Admin)]
        [ResponseType(typeof(PageDefinitionDTO))]
        public PageDefinitionDTO Get(int id)
        {
            var pageDefinition = _pageDefinition.Get(id);
            return Mapper.Map<PageDefinitionDTO>(pageDefinition);
        }
        /// <summary>
        /// Creates or updates specified page definition
        /// </summary>
        /// <param name="pageDefinitionDTO">Page definition to create or update</param>
        [DockyardAuthorize(Roles = Roles.Admin)]
        [SwaggerResponse(HttpStatusCode.NoContent, "Page definition was successfully created or updated")]
        [SwaggerResponseRemoveDefaults]
        public void Post([FromBody]PageDefinitionDTO pageDefinitionDTO)
        {
            if (_securityServices.UserHasPermission(PermissionType.EditPageDefinitions, nameof(PageDefinitionDO)))
            {
                var pageDefinition = Mapper.Map<PageDefinitionDO>(pageDefinitionDTO);
                _pageDefinition.CreateOrUpdate(pageDefinition);
            }
        }
        /// <summary>
        /// Deletes page definition with specified Id
        /// </summary>
        [DockyardAuthorize(Roles = Roles.Admin)]
        [SwaggerResponse(HttpStatusCode.NoContent, "Page definition was successfully deleted")]
        [SwaggerResponseRemoveDefaults]
        public void Delete(int id)
        {
            _pageDefinition.Delete(id);
        }
    }
}
