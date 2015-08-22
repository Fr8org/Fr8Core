using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Core.Helper;
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
        private readonly IAction _action;
        private readonly IActionRegistration _actionRegistration;

        public ActionController()
        {
			_action = ObjectFactory.GetInstance<IAction>();
            _actionRegistration = ObjectFactory.GetInstance<IActionRegistration>();
        }

        public ActionController(IAction service)
        {
            _action = service;
        }

        /*
                public IEnumerable< curActionDesignDTO > Get()
                {
                    return this._action.GetAllActions();
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
                return _action.GetAvailableActions(account);
            }
        }

        /// <summary>
        /// GET : Returns an action with the specified id
        /// </summary>
        [HttpGet]
        public ActionDesignDTO Get(int id)
        {
            return Mapper.Map<ActionDesignDTO>(_action.GetById(id)); 
        }

        /// <summary>
        /// GET : Returns an action with the specified id
        /// </summary>
        [HttpDelete]
        public void Delete(int id)
        {
            _action.Delete(id); 
        }

        /// <summary>
        /// POST : Saves or updates the given action
        /// </summary>
        [HttpPost]
        public IEnumerable<ActionDesignDTO> Save(ActionDesignDTO curActionDesignDTO)
        {
            ActionDO curActionDO = Mapper.Map<ActionDO>(curActionDesignDTO);
            if (_action.SaveOrUpdateAction(curActionDO))
            {
                curActionDesignDTO.Id = curActionDO.Id;
                return new List<ActionDesignDTO> { curActionDesignDTO };
            }
            return new List<ActionDesignDTO>();
        }

        [HttpGet]
        [Route("actions/configuration")]
        public string GetConfigurationSettings(int curActionRegistrationId)
        {
            
            ActionRegistrationDO curActionRegistrationDO = _actionRegistration.GetByKey(curActionRegistrationId);
            return _action.GetConfigurationSettings(curActionRegistrationDO).ConfigurationSettings;
        }


        //retrieve the list of data sources for the drop down list boxes on the left side of the field mapping pane in process builder
        [HttpPost]
        [Route("actions/field_data_sources")]
        public IEnumerable<string> GetFieldDataSources(ActionDesignDTO curActionDesignDTO)
        {
            ActionDO curActionDO = Mapper.Map<ActionDO>(curActionDesignDTO);
            return _action.GetFieldDataSources(curActionDO);
        }

        //retrieve the list of data sources for the text labels on the  right side of the field mapping pane in process builder
        [HttpPost]
        [Route("actions/field_mapping_targets")]
        public Task<IEnumerable<string>> GetFieldMappingTargets(ActionDesignDTO curActionDesignDTO)
        {
            ActionDO curActionDO = Mapper.Map<ActionDO>(curActionDesignDTO);
            return _action.GetFieldMappingTargets(curActionDO);
        }

        [HttpPost]
        [Route("getfieldmapping")]
        public async Task<IEnumerable<string>> GetFieldMapping(ActionDesignDTO actionDto)
        {
            //var actionDto = new ActionDTO() { ParentPluginRegistration = LZString.decompressFromUTF16(pluginName)
            //    , ConfigurationSettings = "{\"connection_string\":\"" + LZString.decompressFromUTF16(connstring) + "\"}" };
            return await _action.GetFieldMappingTargets(Mapper.Map<ActionDesignDTO, ActionDO>(actionDto));
        }

       
    }
}