using System;
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
using Data.Entities;
using Swashbuckle.Swagger.Annotations;
using System;
using log4net;
using Microsoft.AspNet.Identity;
using System.Threading;
using Hub.Exceptions;
using Newtonsoft.Json.Linq;

namespace HubWeb.Controllers
{
    public class TerminalsController : ApiController
    {
        private static readonly ILog Logger = Fr8.Infrastructure.Utilities.Logging.Logger.GetCurrentClassLogger();

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
        [ResponseType(typeof(List<TerminalDTO>))]
        public IHttpActionResult Registrations()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var terminals = uow.TerminalRepository.GetAll()
                    .Select(Mapper.Map<TerminalDTO>)
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
        /// Retrieves the collection of own terminals registered from current user.
        /// In case of Admin user, returns a collection of all terminals   
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <response code="200">Collection of terminals</response>
        /// <response code="403">Unauthorized request</response>
        [HttpGet]
        [Fr8ApiAuthorize]
        [ResponseType(typeof(List<TerminalDTO>))]
        public IHttpActionResult GetByCurrentUser()
        {
            var terminals = _terminal.GetByCurrentUser()
                .Select(Mapper.Map<TerminalDTO>)
                .ToList();
            return Ok(terminals);
        }


        /// <summary>
        /// Retrieves Terminal registered in the current hub by his identifier
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <response code="200">Terminal</response>
        /// <response code="403">Unauthorized request</response>
        [HttpGet]
        [Fr8ApiAuthorize]
        [ResponseType(typeof(TerminalDTO))]
        public IHttpActionResult Get(Guid id)
        {
            var terminalDTO = Mapper.Map<TerminalDTO>(_terminal.GetByKey(id));

            terminalDTO.Roles = _security.GetAllowedUserRolesForSecuredObject(id, nameof(TerminalDO));

            return Ok(terminalDTO);
        }

        /// <summary>
        /// Registers terminal endpoint in the current hub and performs initial terminal discovery process using this endpoint
        /// </summary>
        /// <param name="terminal">Terminal endpoint</param>
        [HttpPost]
        //[Fr8ApiAuthorize]
        [SwaggerResponse(HttpStatusCode.OK, "Terminal has been registered and discovery process has been successfully performed.")]
        [SwaggerResponseRemoveDefaults]
        public async Task<IHttpActionResult> Post([FromBody]TerminalDTO terminal)
        {
            try
            {
                await _terminalDiscovery.SaveOrRegister(terminal);
            }
            catch (ArgumentNullException)
            {
                return BadRequest("An error has occurred while validating terminal data. Please make sure that the form fields are filled out correctly.");
            }
            catch (ArgumentOutOfRangeException)
            {
                return NotFound();
            }
            catch (InvalidOperationException)
            {
                return BadRequest("Terminal URL cannot contain the string 'localhost'. Please correct your terminal URL and try again.");
            }
            catch (ConflictException)
            {
                return Conflict();
            }
            catch (Exception ex)
            {
                var username = Thread.CurrentPrincipal.Identity.GetUserName();
                Logger.Error($"An error has occurred while adding user's terminal on the Terminal's page. Terminal DevURL: {terminal.DevUrl}, ProdURL: {terminal.ProdUrl}, Username: {username}");
                return InternalServerError();
            }
            return Ok();
        }
        /// <summary>
        /// Performs terminal discovery process using endpoint specified
        /// </summary>
        /// <param name="discoveryRef">Terminal endpoint or TerminalDTO</param>
        /// <response code="200">Result of terminal discovery process</response>
        [HttpPost]
        [ResponseType(typeof(ResponseMessageDTO))]
        public async Task<ResponseMessageDTO> ForceDiscover([FromBody] JToken discoveryRef)
        {
            if (discoveryRef == null)
            {
                Logger.Error($"A terminal has submitted the /forcediscovery request with an empty discoveryRef.");
                return ErrorDTO.InternalError("A terminal has submitted the / forcediscovery request with an empty discoveryRef");
            }

            TerminalDTO terminal;

            if (discoveryRef.Type == JTokenType.String)
            {
                terminal = new TerminalDTO
                {
                    Endpoint = discoveryRef.Value<string>()
                };
            }
            else
            {
                terminal = discoveryRef.Value<TerminalDTO>();
            }

            if (!await _terminalDiscovery.Discover(terminal, false))
            {
                return ErrorDTO.InternalError($"Failed to call /discover for endoint {terminal.Endpoint}");
            }

            return new ResponseMessageDTO();
        }
    }
}