using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using StructureMap;
using Core.Interfaces;
using Core.Managers;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;

namespace Web.Controllers
{
    [RoutePrefix("actions")]
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

        [Route("get_configuration")]
        [Route("process")]
        [HttpGet]
        public string HandleDockyardRequest(ActionDesignDTO actionDTO)
        {
            // Extract from current request URL.
            var curActionPath = ActionContext.Request.RequestUri.LocalPath.Substring("/actions/".Length);

            if (curActionPath == "get_configuration")
            {
                curActionPath = "GetConfiguration";
            }

            var curActionDO = Mapper.Map<ActionDO>(actionDTO);

            var curAssemblyName = string.Format("Core.Actions.{0}_v{1}",
                curActionDO.ActionTemplate.Name,
                curActionDO.ActionTemplate.Version);

            var calledType = Type.GetType(curAssemblyName);
            var curMethodInfo = calledType
                .GetMethod(curActionPath, BindingFlags.Default | BindingFlags.IgnoreCase);
            var curObject = Activator.CreateInstance(calledType);

            return JsonConvert.SerializeObject(
                (object)curMethodInfo.Invoke(curObject, new Object[] { curActionDO }) ?? new { });
        }

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
        [Route("{id:int}")]
        public ActionDesignDTO Get(int id)
        {
            return Mapper.Map<ActionDesignDTO>(_action.GetById(id));
        }

        /// <summary>
        /// GET : Returns an action with the specified id
        /// </summary>
        [HttpDelete]
        [Route("{id:int}")]
        public void Delete(int id)
        {
            _action.Delete(id);
        }

        /// <summary>
        /// POST : Saves or updates the given action
        /// </summary>
        [HttpPost]
        [Route("save")]
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
        [ResponseType(typeof(CrateStorageDTO))]
        public IHttpActionResult GetConfigurationSettings(ActionDesignDTO curActionDesignDTO)
        {
            ActionDO curActionDO = Mapper.Map<ActionDO>(curActionDesignDTO);
            return Ok(_action.GetConfigurationSettings(curActionDO));  
        }


        /// <summary>
        /// Retrieve the list of data sources for the drop down list boxes on the left side of the field mapping pane in process builder.
        /// </summary>
        [HttpPost]
        [Route("field_data_sources")]
        public IEnumerable<string> GetFieldDataSources(ActionDesignDTO curActionDesignDTO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curAction = uow.ActionRepository.GetByKey(curActionDesignDTO.Id);

                return _action.GetFieldDataSources(uow, curAction);
            }
        }

        /// <summary>
        /// Retrieve the list of data sources for the text labels on the  right side of the field mapping pane in process builder.
        /// </summary>
        [HttpPost]
        [Route("field_mapping_targets")]
        public string GetFieldMappingTargets(ActionDesignDTO curActionDesignDTO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curAction = uow.ActionRepository.GetByKey(curActionDesignDTO.Id);

                //Field mapping targets are as part of Confgiuration Store of Action DO
                return _action.GetConfigurationSettings(curAction);
            }
        }
    }
}