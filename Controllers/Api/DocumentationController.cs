using System;
using System.Threading.Tasks;
using System.Web.Http;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities;
using Hub.Infrastructure;
using Hub.Interfaces;
using StructureMap;
using System.Web.Http.Description;

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
        /// <summary>
        /// Retrieves documentaion for solutions activities depending on parameters passed
        /// </summary>
        /// <param name="curActivityDTO">Activity to return documentation for</param>
        /// <response code="200">
        /// If activity Documentation property contains value of 'Terminal=[name of terminal]' then list of solution descriptions for specified terminal will be returned. <br />
        /// If activity Documentation property contains value of 'MainPage' then list of solutions descriptions for all terminals will be returned. <br />
        /// If activity Documentation property contains value of 'HelpMenu' then documentation for template of specified activity will be returned
        /// </response>
        /// <response code="400">If specified activity's Documenation doesn't contain non of 'Terminal=[name of terminal]', 'MainPage' or 'HelpMenu'</response>
        [HttpPost]
        [ActionName("activity")]
        [AllowAnonymous]
        [ResponseType(typeof(DocumentationResponseDTO))]
        public async Task<IHttpActionResult> Activity([FromBody] ActivityDTO curActivityDTO)
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
                var solutionPageDTO = await _activity.GetActivityDocumentation<DocumentationResponseDTO>(curActivityDTO, true);
                return Ok(solutionPageDTO);
            }
            if (curDocSupport.Contains("HelpMenu", StringComparison.InvariantCultureIgnoreCase))
            {
                var solutionPageDTO = await _activity.GetActivityDocumentation<DocumentationResponseDTO>(curActivityDTO, false);
                return Ok(solutionPageDTO);
            }
            return BadRequest("Unknown activity documentation request type");
        }
    }
}