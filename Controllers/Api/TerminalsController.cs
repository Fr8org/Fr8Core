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

namespace HubWeb.Controllers
{
	public class TerminalsController : ApiController
	{
        private readonly ISecurityServices _security = ObjectFactory.GetInstance<ISecurityServices>();
        private readonly ITerminal _terminal = ObjectFactory.GetInstance<ITerminal>();
        private readonly ITerminalDiscoveryService _terminalDiscovery = ObjectFactory.GetInstance<ITerminalDiscoveryService>();
        
        [HttpGet]
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

	    [HttpGet]
	    [Fr8ApiAuthorize]
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

        [HttpGet]
        [Fr8ApiAuthorize]
        public IHttpActionResult All()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var terminals = _terminal.GetAll()
                    .Select(Mapper.Map<TerminalDTO>)
                    .ToList();

                return Ok(terminals);
            }
        }

        [HttpPost]
        //[Fr8ApiAuthorize]
        public async Task<IHttpActionResult> Post([FromBody]TerminalRegistrationDTO registration)
		{
		    await _terminalDiscovery.RegisterTerminal(registration.Endpoint);
		    return Ok();
     	}

        [HttpPost]
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