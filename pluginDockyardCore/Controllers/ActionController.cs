using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using Newtonsoft.Json;
using StructureMap;
using Core.Interfaces;
using Core.Managers;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;

namespace pluginDockyardCore.Controllers
{
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private readonly IAction _action;

        public ActionController()
        {
            _action = ObjectFactory.GetInstance<IAction>();
        }

        [HttpGet]
        [Route("configure")]
        public string Configure(ActionDesignDTO curActionDO)
        {
            return HandleDockyardRequest(curActionDO, "Configure");
        }

        [HttpPost]
        [Route("activate")]
        public string Activate(ActionDesignDTO curActionDO)
        {
            return HandleDockyardRequest(curActionDO, "Activate");
        }

        [HttpPost]
        [Route("execute")]
        public string Execute(ActionDesignDTO curActionDO)
        {
            return HandleDockyardRequest(curActionDO, "Execute");
        }

        private string HandleDockyardRequest(ActionDesignDTO actionDTO, string actionPath)
        {
            // Extract from current request URL.
            var curActionDO = Mapper.Map<ActionDO>(actionDTO);

            var curAssemblyName = string.Format("CoreActions.Actions.{0}_v{1}",
                curActionDO.ActionTemplate.Name,
                curActionDO.ActionTemplate.Version);

            var calledType = Type.GetType(curAssemblyName);
            var curMethodInfo = calledType
                .GetMethod(actionPath, BindingFlags.Default | BindingFlags.IgnoreCase);
            var curObject = Activator.CreateInstance(calledType);

            return JsonConvert.SerializeObject(
                (object)curMethodInfo.Invoke(curObject, new Object[] { curActionDO }) ?? new { });
        }
    }
}