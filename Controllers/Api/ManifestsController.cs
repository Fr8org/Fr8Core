using Hub.Interfaces;
using StructureMap;
using System.Web.Http;
using fr8.Infrastructure.Data.Managers;

namespace HubWeb.Controllers
{
    //[RoutePrefix("manifests")]
    public class ManifestsController : ApiController
    {
        private IManifest _manifest;
        private ICrateManager _crateManager;

        public ManifestsController()
        {
            _manifest = ObjectFactory.GetInstance<IManifest>();
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
        }

        [HttpGet]
        //[Route("{id:int}")]
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
