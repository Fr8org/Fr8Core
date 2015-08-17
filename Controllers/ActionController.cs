using System.Collections.Generic;
using System.Linq;
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
using System.Web.Http.Description;

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
        [ResponseType(typeof(IEnumerable<ActionRegistrationDTO>))]
        public IHttpActionResult GetAvailableActions()
        {
            // var userId = User.Identity.GetUserId();
            // using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            // {
            //     var account = uow.UserRepository.GetByKey(userId);
            //     var availableActions = _service
            //         .GetAvailableActions(account)
            //         .Select(x => Mapper.Map<ActionRegistrationDTO>(x))
            //         .ToList();
            // 
            //     return Ok(availableActions);
            // }

            var data = new List<ActionRegistrationDTO>()
            {
                new ActionRegistrationDTO()
                {
                    Id = 1,
                    ActionType = "ActionType One",
                    ParentPluginRegistration = "PluginRegistration One",
                    Version = "1.0.0"
                },
                new ActionRegistrationDTO()
                {
                    Id = 2,
                    ActionType = "ActionType Two",
                    ParentPluginRegistration = "PluginRegistration Two",
                    Version = "1.0.0"
                },
                new ActionRegistrationDTO()
                {
                    Id = 3,
                    ActionType = "ActionType Three",
                    ParentPluginRegistration = "PluginRegistration Three",
                    Version = "1.0.0"
                }
            };

            return Ok(data);
        }

        /// <summary>
        /// GET : Returns an action with the specified id
        /// </summary>
        [HttpGet]
        public ActionDTO Get(int id)
        {
            return Mapper.Map<ActionDTO>(_service.GetById(id)); 
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
        public IEnumerable<ActionDTO> Save(ActionDTO actionVm)
        {
            ActionDO actionDo = Mapper.Map<ActionDO>(actionVm);
            if (_service.SaveOrUpdateAction(actionDo))
            {
                actionVm.Id = actionDo.Id;
                return new List<ActionDTO> { actionVm };
            }
            return new List<ActionDTO>();
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