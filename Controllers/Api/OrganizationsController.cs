using System.Web.Http;
using System.Web.Http.Description;
using Fr8.Infrastructure.Data.DataTransferObjects;
using StructureMap;
using Hub.Infrastructure;
using Hub.Interfaces;
using System;

namespace HubWeb.Controllers
{
    [Fr8ApiAuthorize]
    public class OrganizationsController : ApiController
    {
        private readonly IOrganization _organization;

        public OrganizationsController()
        {
            _organization = ObjectFactory.GetInstance<IOrganization>();
        }
        /// <summary>
        /// Retrieves description of organization with specified Id
        /// </summary>
        /// <param name="id">Id of organization</param>
        /// <response code="200">Organization with specified Id. Can be empty</response>
        [HttpGet]
        [ResponseType(typeof(OrganizationDTO))]
        public IHttpActionResult Get(int id)
        {
            return Ok(_organization.GetOrganizationById(id));
        }
        /// <summary>
        /// Updates specified organization
        /// </summary>
        /// <param name="dto">Organization to update</param>
        /// <response code="200">Updated organization</response>
        [HttpPut]
        [ResponseType(typeof(OrganizationDTO))]
        public IHttpActionResult Put(OrganizationDTO dto)
        {
            return Ok(_organization.UpdateOrganization(dto));
        }
    }
}