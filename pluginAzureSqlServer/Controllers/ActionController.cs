using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using Data.Entities;
using PluginBase.BaseClasses;
using System.Collections.Generic;
using Data.States;
using System;

namespace pluginAzureSqlServer.Controllers
{    
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private const string curPlugin = "pluginAzureSqlServer";
        private BasePluginController _basePluginController = new BasePluginController();

        [HttpPost]
        [Route("configure")]
        public string Configure(ActionDataPackageDTO curActionDataPackage)
        {
            return _basePluginController.HandleDockyardRequest(curPlugin, "Configure", curActionDataPackage);
        }
       
        [HttpPost]
        [Route("activate")]
        public string Activate(ActionDataPackageDTO curActionDataPackage)
        {
            return string.Empty;
        }

        [HttpPost]
        [Route("execute")]
        public string Execute(ActionDataPackageDTO curActionDataPackage)
        {
            return string.Empty;
        }



        //----------------------------------------------------------


        [HttpPost]
        [Route("Write_To_Sql_Server")]
        [Obsolete]
        public IHttpActionResult Process(ActionDTO curActionDTO)
        {
            //var _actionHandler = ObjectFactory.GetInstance<Write_To_Sql_Server_v1>();
            //ActionDO curAction = Mapper.Map<ActionDO>(curActionDTO);
            return
                Ok("This end point has been deprecated. Please use the V2 mechanisms to POST to this plugin. For more" +
                   "info see https://maginot.atlassian.net/wiki/display/SH/V2+Plugin+Design");

        }

        [HttpPost]
        [Route("Write_To_Sql_Server/{path}")]
        public IHttpActionResult Process(string path, ActionDTO curActionDTO)

        {
            //ActionDO curAction = Mapper.Map<ActionDO>(curActionDTO);

            //string[] curUriSplitArray = Url.Request.RequestUri.AbsoluteUri.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            //string curAssemblyName = string.Format("pluginAzureSqlServer.Actions.{0}_v{1}", curUriSplitArray[curUriSplitArray.Length - 2], "1");
            ////extract the leading element of the path, which is the current Action and will be something like "write_to_sql_server"
            ////instantiate the class corresponding to that action by:
            ////   a) Capitalizing each word
            ////   b) adding "_v1" onto the end (this will get smarter later)
            ////   c) Create an instance. probably create a Type using the string and then use ObjectFactory.GetInstance<T>. you want to end up with this:
            ////            "_actionHandler = ObjectFactory.GetInstance<Write_To_Sql_Server_v1>();"
            ////   d) call that instance's Process method, passing it the remainder of the path and an ActionDO


            ////Redirects to the action handler with fallback in case of a null retrn

            //Type calledType = Type.GetType(curAssemblyName);
            //MethodInfo curMethodInfo = calledType.GetMethod("Process");
            //object curObject = Activator.CreateInstance(calledType);

            //return JsonConvert.SerializeObject(
            //    //_actionHandler.Process(path, curAction) ?? new { }
            //    (object)curMethodInfo.Invoke(curObject, new Object[] { path, curAction }) ?? new { }
            //);

            return
                Ok("This end point has been deprecated. Please use the V2 mechanisms to POST to this plugin. For more" +
                   "info see https://maginot.atlassian.net/wiki/display/SH/V2+Plugin+Design");

        }

        [HttpGet]
        [Route("action_templates")]
        public IHttpActionResult ActionTemplates()
        {
            var result = new List<ActionTemplateDO>();
            var template = new ActionTemplateDO
            {
                Name = "WriteToAzureSqlServer",
                Version = "1.0",
                ActionProcessor = "DockyardAzureSqlServerService",
            };
            var plugin = new PluginDO
            {
                BaseEndPoint = "localhost:46281",
                Endpoint = "localhost:46281",
                PluginStatus = PluginStatus.Active,
                Name = template.Name
            };
            template.Plugin = plugin;
            result.Add(template);
            return Json(result);           
        }
    }
}