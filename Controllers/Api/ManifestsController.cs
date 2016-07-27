using Hub.Interfaces;
using StructureMap;
using System.Web.Http;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.DataTransferObjects;
using System.Web.Http.Description;
using Fr8.Infrastructure.Data.Manifests;

namespace HubWeb.Controllers
{
    public class ManifestsController : ApiController
    {
        private IManifest _manifest;
        private ICrateManager _crateManager;

        public ManifestsController()
        {
            _manifest = ObjectFactory.GetInstance<IManifest>();
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
        }
        /// <summary>
        /// Gets crate that contains information about all properties of manifest with specified Id
        /// </summary>
        /// <param name="id">Id of manifest</param>
        /// <response code="200">Crate with FieldDescriptionsCM manifest. Can be empty</response>
        [HttpGet]
        [ResponseType(typeof(CrateDTO))]
        public IHttpActionResult Get(int id)
        {
            var crate = _manifest.GetById(id);
            if (crate != null)
            {
                return Ok(_crateManager.ToDto(crate));
            }
            return Ok();
        }
    }
}
