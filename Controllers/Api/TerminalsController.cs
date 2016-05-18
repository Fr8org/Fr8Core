using System.Linq;
using System.Web.Http;
using AutoMapper;
using Data.Entities;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Validations;
using Fr8Data.DataTransferObjects;
using Hub.Infrastructure;
using Hub.Interfaces;
using StructureMap;

namespace HubWeb.Controllers
{
    [Fr8ApiAuthorize]
	public class TerminalsController : ApiController
	{
        private readonly ISecurityServices _security = ObjectFactory.GetInstance<ISecurityServices>();
        private readonly ITerminal _terminal = ObjectFactory.GetInstance<ITerminal>();
        
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

		[HttpPost]
		public IHttpActionResult Post(TerminalDTO terminalDto)
		{
            TerminalDO terminal = Mapper.Map<TerminalDO>(terminalDto);

            var validator = new TerminalValidator();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (terminalDto == null || !validator.Validate(terminalDto).IsValid)
                {
                    return BadRequest("Some of the request data is invalid");
                }

                terminal.Version = "1";
                terminal.UserDO = _security.GetCurrentAccount(uow);

                terminal = _terminal.RegisterOrUpdate(terminal);
                

                var subscriptionDO = new TerminalSubscriptionDO
                {
                    TerminalId = terminal.Id,
                    UserDO = terminal.UserDO
                };
                uow.TerminalSubscriptionRepository.Add(subscriptionDO);
                uow.SaveChanges();
            }
            EventManager.Fr8AccountTerminalRegistration(terminal);

            var model = Mapper.Map<TerminalDTO>(terminal);

			return Ok(model);
		}
	}
}