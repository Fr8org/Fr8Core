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
			_service = ObjectFactory.GetInstance<IAction>();
        }

        public ActionController(IAction service)
        {
            _service = service;
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
        /// GET : Returns an action with the specified id
        /// </summary>
        [HttpGet]
        public ActionDesignDTO Get(int id)
        {
            return Mapper.Map<ActionDesignDTO>(_service.GetById(id)); 
        }

        /// <summary>
        /// GET : Returns an action with the specified id
        /// </summary>
        [HttpDelete]
        public void Delete(int id)
        {
            _service.Delete(id); 
        }

        /// <summary>
        /// POST : Saves or updates the given action
        /// </summary>
        [HttpPost]
        public IEnumerable<ActionDesignDTO> Save(ActionDesignDTO actionVm)
        {
            ActionDO actionDo = Mapper.Map<ActionDO>(actionVm);
            if (_service.SaveOrUpdateAction(actionDo))
            {
                actionVm.Id = actionDo.Id;
                return new List<ActionDesignDTO> { actionVm };
            }
            return new List<ActionDesignDTO>();
        }

        [HttpGet]
        [Route("actions/configuration")]
        public string GetConfigurationSettings(int curActionRegistrationId)
        {
            IActionRegistration _actionRegistration = new ActionRegistration();
            ActionRegistrationDO curActionRegistrationDO = _actionRegistration.GetByKey(curActionRegistrationId);
            return _service.GetConfigurationSettings(curActionRegistrationDO).ConfigurationSettings;
        }
    }
}