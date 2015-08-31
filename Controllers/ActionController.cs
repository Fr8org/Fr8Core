using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using Core.Interfaces;
using Core.Managers;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Microsoft.AspNet.Identity;
using StructureMap;
using Core.PluginRegistrations;

namespace Web.Controllers
{
    [RoutePrefix("api/actions")]
    public class ActionController : ApiController
    {
        private readonly IAction _action;
        private readonly IActionTemplate _actionTemplate;

        public ActionController()
        {
			_action = ObjectFactory.GetInstance<IAction>();
            _actionTemplate = ObjectFactory.GetInstance<IActionTemplate>();
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
        [ResponseType(typeof(IEnumerable<ActionTemplateDTO>))]
        public IHttpActionResult GetAvailableActions()
        {
            var userId = User.Identity.GetUserId();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curDockyardAccount = uow.UserRepository.GetByKey(userId);
                var availableActions = _action
                    .GetAvailableActions(curDockyardAccount)
                    .Select(x => Mapper.Map<ActionTemplateDTO>(x))
                    .ToList();
            
                return Ok(availableActions);
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

        [HttpPost]
        [Route("actions/configuration")]
        public string GetConfigurationSettings(ActionDesignDTO curActionDesignDTO)
        {            
            ActionDO curActionDO = Mapper.Map<ActionDO>(curActionDesignDTO);
            if (curActionDO.ActionTemplate != null)
            {
                var _pluginRegistration = ObjectFactory.GetInstance<IPluginRegistration>();
                string typeName = _pluginRegistration.AssembleName(curActionDO.ActionTemplate);
                var settings = _pluginRegistration.CallPluginRegistrationByString(typeName, "GetConfigurationSettings", curActionDO);
                return settings;
            }
            else
            {
                throw new System.ArgumentNullException("ActionTemplate is null");
            }
           
        }


        //retrieve the list of data sources for the drop down list boxes on the left side of the field mapping pane in process builder
        [HttpPost]
        [Route("field_data_sources")]
        public IEnumerable<string> GetFieldDataSources(ActionDesignDTO curActionDesignDTO)
        {
            ActionDO curActionDO = Mapper.Map<ActionDO>(curActionDesignDTO);
            return _action.GetFieldDataSources(curActionDO);
        }

        //retrieve the list of data sources for the text labels on the  right side of the field mapping pane in process builder
        [HttpPost]
        [Route("field_mapping_targets")]
        public Task<IEnumerable<string>> GetFieldMappingTargets(ActionDesignDTO curActionDesignDTO)
        {
            ActionDO curActionDO = Mapper.Map<ActionDO>(curActionDesignDTO);
            return _action.GetFieldMappingTargets(curActionDO);
        }


       
    }
}