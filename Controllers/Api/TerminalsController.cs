using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Hub.Infrastructure;
using Hub.Interfaces;
using StructureMap;
using System.Collections.Generic;
using System.Net;
using System.Web.Http.Description;
using Swashbuckle.Swagger.Annotations;

namespace HubWeb.Controllers
{
    public class TerminalsController : ApiController
    {
        private readonly ISecurityServices _security = ObjectFactory.GetInstance<ISecurityServices>();
        private readonly ITerminal _terminal = ObjectFactory.GetInstance<ITerminal>();
        private readonly ITerminalDiscoveryService _terminalDiscovery = ObjectFactory.GetInstance<ITerminalDiscoveryService>();
        /// <summary>
        /// Retrieves the collection of terminals registered in the current hub by current user
        /// </summary>
        /// <response code="200">Collection of terminals</response>
        [HttpGet]
        [ResponseType(typeof(List<TerminalDTO>))]
        public IHttpActionResult Get()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var currentUser = _security.GetCurrentAccount(uow);
                var models = _terminal.GetAll()
                    .Where(u => u.UserDO != null && u.UserDO.Id == currentUser.Id)
                    .Select(Mapper.Map<TerminalDTO>)
                    .ToList();

                return Ok(models);
            }
        }
        /// <summary>
        /// Retrieves the collection of terminal endpoints that are registered in the current hub
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <response code="200">Collection of terminal endpoints</response>
        /// <response code="403">Unauthorized request</response>
	    [HttpGet]
        [Fr8ApiAuthorize]
        [ResponseType(typeof(List<TerminalRegistrationDTO>))]
        public IHttpActionResult Registrations()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var terminals = uow.TerminalRegistrationRepository.GetAll()
                    .Select(Mapper.Map<TerminalRegistrationDTO>)
                    .ToList();

                return Ok(terminals);
            }
        }
        /// <summary>
        /// Retrieves the collection of all terminals registered in the current hub
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <response code="200">Collection of terminals</response>
        /// <response code="403">Unauthorized request</response>
        [HttpGet]
        [Fr8ApiAuthorize]
        [ResponseType(typeof(List<TerminalDTO>))]
        public IHttpActionResult All()
        {
            var terminals = _terminal.GetAll()
                .Select(Mapper.Map<TerminalDTO>)
                .ToList();
            return Ok(terminals);
        }
        /// <summary>
        /// Registers terminal endpoint in the current hub and performs initial terminal discovery process using this endpoint
        /// </summary>
        /// <param name="registration">Terminal endpoint</param>
        [HttpPost]
        //[Fr8ApiAuthorize]
        [SwaggerResponse(HttpStatusCode.OK, "Terminal was registered and discovery process was successfully performed")]
        [SwaggerResponseRemoveDefaults]
        public async Task<IHttpActionResult> Post([FromBody]TerminalRegistrationDTO registration)
        {
            await _terminalDiscovery.RegisterTerminal(registration.Endpoint);
            return Ok();
        }
        /// <summary>
        /// Performs terminal discovery process using endpoint specified
        /// </summary>
        /// <param name="callbackUrl">Terminal endpoint</param>
        /// <response code="200">Result of terminal discovery process</response>
        [HttpPost]
        [ResponseType(typeof(ResponseMessageDTO))]
        public async Task<ResponseMessageDTO> ForceDiscover([FromBody] string callbackUrl)
        {
            if (!await _terminalDiscovery.Discover(callbackUrl))
            {
                return ErrorDTO.InternalError($"Failed to call /discover for enpoint {callbackUrl}");
            }
            return new ResponseMessageDTO();
        }
    }
}