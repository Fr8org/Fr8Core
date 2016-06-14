using System.Web.Http;
using System.Web.Http.Description;
using Fr8.Infrastructure.Data.DataTransferObjects;
using StructureMap;
using Hub.Infrastructure;
using Hub.Interfaces;

namespace HubWeb.Controllers
{
    [Fr8ApiAuthorize]
    public class OrganizationsController: ApiController
	{
        private readonly IOrganization _organization;

        public OrganizationsController()
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