using System.Collections.Generic;
using System.Web.Http;
using AutoMapper;
using Core.Interfaces;
using Core.Managers;
using Data.Interfaces;
using Microsoft.AspNet.Identity;
using StructureMap;
using Web.ViewModels;
using Core.Services;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;

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
        public IEnumerable<string> GetAvailableActions()
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
        public IEnumerable<ActionVM> Save(ActionVM actionVm)
        {
            if (_service.SaveOrUpdateAction(Mapper.Map<ActionDO>(actionVm)))
            {
                return new List<ActionVM> { actionVm };
            }
            return new List<ActionVM>();
        }

        [HttpGet]
        [Route("api/actions/configuration")]
        public ActionDTO GetConfigurationSetting(int curActionRegistrationDOId)
        {
            IActionRegistration _serviceActionRegistration = new ActionRegistration();
            ActionRegistrationDO curActionRegistrationDO = _serviceActionRegistration.GetByKey(curActionRegistrationDOId);
            ActionDTO curActionDTO = new ActionDTO();
            curActionDTO.ConfigurationSettings = _service.GetConfigurationSettings(curActionRegistrationDO).ConfigurationSettings;
            return curActionDTO;
        }
    }
}