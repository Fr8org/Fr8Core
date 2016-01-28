using System.Linq;
using System.Web.Http;
using AutoMapper;
using Data.Entities;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Validations;
using StructureMap;

namespace HubWeb.Controllers
{
    [Fr8ApiAuthorize]
	public class TerminalsController : ApiController
	{
        private readonly ISecurityServices _security = ObjectFactory.GetInstance<ISecurityServices>();

        [HttpGet]
		public IHttpActionResult Get()
		{
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var currentUser = _security.GetCurrentAccount(uow);
                var models = uow.TerminalRepository.GetAll()
                    .Where(u => u.UserDO != null && u.UserDO.Id == currentUser.Id)
                    .Select(Mapper.Map<TerminalDTO>)
                    .ToList();

                return Ok(models);
            }
		}

		[HttpPost]
		public IHttpActionResult Post(TerminalDTO terminal)
		{
            TerminalDO entity = Mapper.Map<TerminalDO>(terminal);

            var validator = new TerminalValidator();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (terminal == null || !validator.Validate(terminal).IsValid)
                {
                    return BadRequest("Some of the request data is invalid");
                }

                entity.Version = "1";
                entity.UserDO = _security.GetCurrentAccount(uow);
                uow.TerminalRepository.Add(entity);

				uow.SaveChanges();

                var subscriptionDO = new TerminalSubscriptionDO
                {
                    TerminalId = entity.Id,
                    UserDO = entity.UserDO
                };
                uow.TerminalSubscriptionRepository.Add(subscriptionDO);
                uow.SaveChanges();
            }
            EventManager.Fr8AccountTerminalRegistration(entity);

            var model = Mapper.Map<TerminalDTO>(entity);

			return Ok(model);
		}
	}
}