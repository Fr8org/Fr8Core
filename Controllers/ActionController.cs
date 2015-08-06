using System.Collections.Generic;
using System.Web.Http;
using Core.Interfaces;
using Core.Managers;
using Data.Interfaces;
using Microsoft.AspNet.Identity;
using StructureMap;
using Web.Controllers.Services;
using Web.ViewModels;

namespace Web.Controllers
{
    public class ActionController : ApiController
    {
        private readonly IActionsService _service;

        public ActionController()
        {
            this._service = new ActionsService(ObjectFactory.GetInstance<ISubscriptionService>());
        }

        /*
                public IEnumerable< ActionVM > Get()
                {
                    return this._service.GetAllActions();
                }
        */

        [KwasantAuthorize]
        public IEnumerable<string> GetAvailableActions()
        {
            var userId = this.User.Identity.GetUserId();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var account = uow.UserRepository.GetByKey(userId);
                return this._service.GetAvailableActions(account);
            }
        }

        /// <summary>
        /// POST : Saves or updates the given action
        /// </summary>
        [HttpPost]
        public IEnumerable<ActionVM> Save(ActionVM actionVm)
        {
            if (_service.SaveOrUpdateAction(actionVm))
            {
                return new List<ActionVM> { actionVm };
            }
            return new List<ActionVM>();
        }
    }
}
