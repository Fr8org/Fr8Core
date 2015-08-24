using System;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json.Linq;
using pluginAzureSqlServer.Infrastructure;
using pluginAzureSqlServer.Services;
using PluginUtilities.Infrastructure;
using AutoMapper;
using Data.Entities;
//using Utilities.Serializers.Json;
using System.Collections.Generic;
using Newtonsoft.Json;
using StructureMap;
using System.Data.SqlClient;
using System.Data;
using pluginAzureSqlServer.Actions;
using PluginUtilities;
using System.Reflection;


namespace pluginAzureSqlServer.Controllers
{    
    [RoutePrefix("plugin_azure_sql_server/actions")]
    public class ActionController : ApiController
    {       
        //public const string Version = "1.0";

        //private readonly Write_To_Sql_Server_v1 _actionHandler;

        public ActionController() {
            //_actionHandler = ObjectFactory.GetInstance<Write_To_Sql_Server_v1>(); //remove this initialization once Process is modified, below
        }

        [HttpPost]
        [Route("Write_To_Sql_Server/{path}")]
        public string Process(string path, ActionDO curActionDTO)
        {

            ActionDO curAction = Mapper.Map<ActionDO>(curActionDTO);
            string[] curUriSplitArray = Url.Request.RequestUri.AbsoluteUri.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string curAssemblyName = string.Format("pluginAzureSqlServer.Actions.{0}_v{1}", curUriSplitArray[curUriSplitArray.Length - 2], "1");
            //extract the leading element of the path, which is the current Action and will be something like "write_to_sql_server"
            //instantiate the class corresponding to that action by:
            //   a) Capitalizing each word
            //   b) adding "_v1" onto the end (this will get smarter later)
            //   c) Create an instance. probably create a Type using the string and then use ObjectFactory.GetInstance<T>. you want to end up with this:
            //            "_actionHandler = ObjectFactory.GetInstance<Write_To_Sql_Server_v1>();"
            //   d) call that instance's Process method, passing it the remainder of the path and an ActionDO


            //Redirects to the action handler with fallback in case of a null retrn

            Type calledType = Type.GetType(curAssemblyName);
            MethodInfo curMethodInfo = calledType.GetMethod("Process");
            object curObject = Activator.CreateInstance(calledType);

            return JsonConvert.SerializeObject(
                //_actionHandler.Process(path, curAction) ?? new { }
                (object)curMethodInfo.Invoke(curObject, new Object[] { path, curActionDTO }) ?? new { }
            );
        }

       



    }
}