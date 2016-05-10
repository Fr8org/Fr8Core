using System.Web.Http;
using System.Web.Http.Description;
using StructureMap;
using Data.Interfaces.DataTransferObjects;
using Hub.Infrastructure;
using Hub.Interfaces;

namespace HubWeb.Controllers
{
    [Fr8ApiAuthorize]
    public class OrganizationController: ApiController
	{
        private readonly IOrganization _organization;

        public OrganizationController()
        {
            _organization = ObjectFactory.GetInstance<IOrganization>();
        }

        [HttpGet]
        public IHttpActionResult Get(int id)
        {
            return Ok(_organization.GetOrganizationById(id));
        }
        
        
        [ResponseType(typeof(OrganizationDTO))]
        [HttpPut]
        public IHttpActionResult Put(OrganizationDTO dto)
        {
            return Ok(_organization.UpdateOrganization(dto));
        }
    }
}