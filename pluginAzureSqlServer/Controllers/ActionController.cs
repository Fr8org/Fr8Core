using Data.Interfaces.DataTransferObjects;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using terminal_base.BaseClasses;

namespace terminal_AzureSqlServer.Controllers
{    
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private const string curTerminal = "terminal_AzureSqlServer";
        private BaseTerminalController _baseTerminalController = new BaseTerminalController();

        [HttpPost]
        [Route("configure")]
        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            return await (Task<ActionDTO>) _baseTerminalController
                .HandleDockyardRequest(curTerminal, "Configure", curActionDTO);           
        }
       
        [HttpPost]
        [Route("activate")]
        public ActionDTO Activate(ActionDTO curActionDataPackage)
        {
            return (ActionDTO)_baseTerminalController.HandleDockyardRequest(curTerminal, "Activate", curActionDataPackage);
        }

        [HttpPost]
        [Route("deactivate")]
        public ActionDTO Deactivate(ActionDTO curActionDataPackage)
        {
            return (ActionDTO)_baseTerminalController.HandleDockyardRequest(curTerminal, "Deactivate", curActionDataPackage);
        }

        [HttpPost]
        [Route("execute")]
        public async Task<PayloadDTO> Execute(ActionDTO actionDto)
        {
            return await (Task<PayloadDTO>)_baseTerminalController.HandleDockyardRequest(
                curTerminal, "Execute", actionDto);
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
                Ok("This end point has been deprecated. Please use the V2 mechanisms to POST to this terminal. For more" +
                   "info see https://maginot.atlassian.net/wiki/display/SH/V2+Plugin+Design");

        }

        [HttpPost]
        [Route("Write_To_Sql_Server/{path}")]
        public IHttpActionResult Process(string path, ActionDTO curActionDTO)

        {
            //ActionDO curAction = Mapper.Map<ActionDO>(curActionDTO);

            //string[] curUriSplitArray = Url.Request.RequestUri.AbsoluteUri.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            //string curAssemblyName = string.Format("terminalAzureSqlServer.Actions.{0}_v{1}", curUriSplitArray[curUriSplitArray.Length - 2], "1");
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