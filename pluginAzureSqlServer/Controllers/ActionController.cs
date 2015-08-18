using System;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json.Linq;
using pluginAzureSqlServer.Infrastructure;
using pluginAzureSqlServer.Messages;
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
using PluginUtilities;


namespace pluginAzureSqlServer.Controllers
{    
    [RoutePrefix("plugin_azure_sql_server/actions")]
    public class ActionController : ApiController
    {       
        //public const string Version = "1.0";

        private readonly AzureSqlServerActionHandler _actionHandler;

        public ActionController() {
            _actionHandler = ObjectFactory.GetInstance<AzureSqlServerActionHandler>();
        }

        //private readonly IDbProvider _dbProvider;
        //private readonly JsonSerializer _serializer;

        //public ActionController(IDbProvider dbProvider, JsonSerializer serializer) {
        //    _dbProvider = dbProvider;
        //    _serializer = serializer;
        //}

        /// <summary>
        /// Insert user data to remote database tables.
        /// </summary>
        //[HttpPost]
        //[Route("writeSQL")]
        //public CommandResponse Write(JObject data)
        //{                 
        //    try
        //    {
        //        // Creating ExtrationHelper and parsing WriteCommandArgs.
        //        var parser = new DbServiceJsonParser();
        //        var writeArgs = parser.ExtractWriteCommandArgs(data);

        //        // Creating DbService and running WriteCommand logic.
        //        var dbService = new DbService();
        //        dbService.WriteCommand(writeArgs);
        //    }
        //    catch (Exception ex)
        //    {
        //        return CommandResponse.ErrorResponse(ex.Message);
        //    }

        //    return CommandResponse.SuccessResponse();
        //}


        [HttpPost]
        [Route("write_to_sql_server/{path}")]
        public string WriteToSqlServer(string path, ActionDTO curActionDTO) {            
            //Redirects to the action handler with fallback in case of a null retrn
            return JsonConvert.SerializeObject(
                _actionHandler.WriteToSqlServerAction(path, Mapper.Map<ActionDO>(curActionDTO)) ?? new { }
            );
        }

    }
}