using System;
using System.Threading.Tasks;
using System.Web.Http;
using Fr8Data.DataTransferObjects;
using Hub.Interfaces;
using StructureMap;
using Utilities;

namespace HubWeb.Controllers
{
    [Fr8ApiAuthorize]
    public class DocumentationController : ApiController
    {
        private readonly IActivity _activity;

        private readonly ITerminal _terminal;
        //TODO: remove this contsructor when we start using constructor injection
        public DocumentationController()
        {
            _activity = ObjectFactory.GetInstance<IActivity>();
            _terminal = ObjectFactory.GetInstance<ITerminal>();
        }
        
        public DocumentationController(IActivity activity, ITerminal terminal)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }
            if (terminal == null)
            {
                throw new ArgumentNullException(nameof(terminal));
            }
            _terminal = terminal;
            _activity = activity;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("activity")]
        public async Task<IHttpActionResult> ActivityDocumentation([FromBody] ActivityDTO curActivityDTO)
        {
            var curDocSupport = curActivityDTO.Documentation;
            if (curDocSupport.StartsWith("Terminal=", StringComparison.InvariantCultureIgnoreCase))
            {
                var terminalName = curDocSupport.Split('=')[1];
                var solutionPages = await _terminal.GetSolutionDocumentations(terminalName);
                return Ok(solutionPages);
            }
            if (curDocSupport.Contains("MainPage", StringComparison.InvariantCultureIgnoreCase))
            {
                var solutionPageDTO = await _activity.GetActivityDocumentation<SolutionPageDTO>(curActivityDTO, true);
                return Ok(solutionPageDTO);
            }
            return BadRequest("Unknown activity documentation request type");
        }
    }
}