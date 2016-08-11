using Data.Interfaces;
using StructureMap;
using System.Web.Http;
using Data.Entities;
using Data.Infrastructure.StructureMap;

namespace HubWeb.Controllers
{
    public class Fr8BaseApiController : ApiController
    {
        protected readonly ISecurityServices _security;

        public Fr8BaseApiController(ISecurityServices security)
        {
            _security = security;
        }

        protected bool IsThisTerminalCall()
        {
            return User.Identity.Name.StartsWith("terminal-");
        }

        protected Fr8AccountDO GetUserTerminalOperatesOn()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return _security.GetCurrentAccount(uow);
            }
        }
    }
}