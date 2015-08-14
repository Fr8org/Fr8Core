using System.Collections.Generic;
using System.Web.Http;
using AutoMapper;
using Microsoft.AspNet.Identity;
using StructureMap;
using Core.Interfaces;
using Core.Managers;
using Core.Services;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Web.ViewModels;

namespace Web.Controllers
{
    [RoutePrefix("api/actions")]
    public class ActionController : ApiController
    {
        private readonly IAction _service;

        public ActionController()
        {
			_service = new Action();
        }

        /*
                public IEnumerable< ActionVM > Get()
                {
                    return this._service.GetAllActions();
                }
        */

        [DockyardAuthorize]
        [Route("available")]
        public IEnumerable<ActionRegistrationDO> GetAvailableActions()
        {
            var userId = User.Identity.GetUserId();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var account = uow.UserRepository.GetByKey(userId);
                return _service.GetAvailableActions(account);
            }
        }

        /// <summary>
        /// POST : Saves or updates the given action
        /// </summary>
        [HttpPost]
        public IEnumerable<ActionDTO> Save(ActionDTO actionVm)
        {
            if (_service.SaveOrUpdateAction(Mapper.Map<ActionDO>(actionVm)))
            {
                return new List<ActionDTO> { actionVm };
            }
            return new List<ActionDTO>();
        }
    }
}