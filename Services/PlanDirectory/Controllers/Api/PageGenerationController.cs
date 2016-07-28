using System.Threading.Tasks;
using System.Web.Http;
using PlanDirectory.Interfaces;
using StructureMap;

namespace PlanDirectory.Controllers.Api
{
    public class PageGenerationController : ApiController
    {
        private readonly IManifestPageGenerator _manifestPageGenerator;
        public PageGenerationController()
        {
            _manifestPageGenerator = ObjectFactory.GetInstance<IManifestPageGenerator>();
        }

        [HttpPost]
        [ActionName("generate_manifest_page")]
        public async Task<IHttpActionResult> GenerateManifestPage([FromBody] string manifestName)
        {
            return Ok(await _manifestPageGenerator.Generate(manifestName, GenerateMode.GenerateIfNotExists));
        }
    }
}
