using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Microsoft.AspNet.Identity;
using StructureMap;
using Core.Interfaces;
using Core.Managers;
using Core.PluginRegistrations;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Web.ViewModels;
using Action = Core.Services.Action;

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

        [HttpGet]
        [Route("fieldmappings")]
        public async Task<IEnumerable<string>> GetFieldMappingTargets(ActionDTO curAction)
        {
            return await _service.GetFieldMappingTargets(Mapper.Map<ActionDTO, ActionDO>(curAction));
        }
    }
}