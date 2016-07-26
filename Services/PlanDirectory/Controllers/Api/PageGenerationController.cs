using System.Threading.Tasks;
using System.Web.Http;
using PlanDirectory.Interfaces;

namespace PlanDirectory.Controllers.Api
{
    public class PageGenerationController : ApiController
    {
        private readonly IManifestPageGenerator _manifestPageGenerator;
        public PageGenerationController(IManifestPageGenerator manifestPageGenerator)
        {
            _manifestPageGenerator = manifestPageGenerator;
        }

        [HttpPost]
        [ActionName("generate_manifest_page")]
        public async Task<IHttpActionResult> GenerateManifestPage([FromBody] string manifestName)
        {
            return Ok(await _manifestPageGenerator.Generate(manifestName, GenerateMode.GenerateIfNotExists));
        }
    }
}
