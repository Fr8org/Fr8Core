using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using Data.Entities;
using Data.States;
using TerminalBase.BaseClasses;
using AutoMapper;

namespace terminalAzure.Controllers
{    
    [RoutePrefix("actions")]
    public class ActionController : BaseTerminalController
    {
        private const string curTerminal = "terminalAzure";

        [HttpPost]
        public Task<object> Execute([FromUri] String actionType, [FromBody] ActionDTO curActionDTO)
        {
            return HandleFr8Request(curTerminal, actionType, curActionDTO);
        }

        //----------------------------------------------------------


        [HttpPost]
        [Route("Write_To_Sql_Server")]
        [Obsolete]
        public IHttpActionResult Process(ActionDO curActionDO)
        {
            //var _actionHandler = ObjectFactory.GetInstance<Write_To_Sql_Server_v1>();
            //ActionDO curAction = Mapper.Map<ActionDO>(curActionDTO);
            return
                Ok("This end point has been deprecated. Please use the V2 mechanisms to POST to this terminal. For more" +
                   "info see https://maginot.atlassian.net/wiki/display/SH/V2+Plugin+Design");

        }

        [HttpPost]
        [Route("Write_To_Sql_Server/{path}")]
        public IHttpActionResult Process(string path, ActionDO curActionDO)

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
                Ok("This end point has been deprecated. Please use the V2 mechanisms to POST to this terminal. For more" +
                   "info see https://maginot.atlassian.net/wiki/display/SH/V2+Plugin+Design");

        }
    }
}