using System;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json.Linq;
using pluginDocuSign.Infrastructure;
using pluginDocuSign.Services;
using PluginUtilities.Infrastructure;
using AutoMapper;
using Data.Entities;
//using Utilities.Serializers.Json;
using System.Collections.Generic;
using Newtonsoft.Json;
using StructureMap;
using System.Data.SqlClient;
using System.Data;
using pluginDocuSign.Actions;
using PluginUtilities;
using System.Reflection;


namespace pluginDocuSign.Controllers
{    
    [RoutePrefix("plugin_docusign/actions")]
    public class ActionController : ApiController
    {
        public ActionController()
        {
        }

        //[Route("{action}")]
        public string HandleDockyardRequest(string action, ActionDesignDTO curActionDesigndDTO)
        {
            ActionDO curActionDO = Mapper.Map<ActionDO>(curActionDesigndDTO);
            string curClassName = curActionDesigndDTO.ActionName;
            string curVersion = curActionDesigndDTO.ActionVersion;

            //string curAssemblyName = string.Format("pluginDocuSign.Actions.{0}_v{1}", curUriSplitArray[curUriSplitArray.Length - 2], "1");
            string curAssemblyName = string.Format("pluginDocuSign.Actions.{0}_v{1}", curClassName, curVersion);

            //Redirects to the action handler with fallback in case of a null retrn
            
            Type calledType = Type.GetType(curAssemblyName);
            MethodInfo curMethodInfo = calledType.GetMethod(String.Format("Handle{0}Request", action));
            object curObject = Activator.CreateInstance(calledType);

            return JsonConvert.SerializeObject(
                (object)curMethodInfo.Invoke(curObject, new Object[] { curActionDO }) ?? new { }
            );
        }        

        [HttpPost]
        [Route("configure")]
        public string Configure(ActionDesignDTO curActionDesignDTO)
        {
            //return HandleDockyardRequest("Configure", curActionDesignDTO);
            return "Configure Request"; // Will be changed when implementation is plumbed in.
        }

        [HttpPost]
        [Route("activate")]
        public string Actvate(ActionDesignDTO curActionDesignDTO)
        {
            //return HandleDockyardRequest("Actvate", curActionDesignDTO);
            return "Activate Request"; // Will be changed when implementation is plumbed in.
        }

        [HttpPost]
        [Route("execute")]
        public string Execute(ActionDesignDTO curActionDesignDTO)
        {
            //return HandleDockyardRequest("Execute", curActionDesignDTO);
            return "Execute Request"; // Will be changed when implementation is plumbed in.
        }
    }
}