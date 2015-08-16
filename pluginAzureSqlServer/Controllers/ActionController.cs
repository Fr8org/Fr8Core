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
        protected delegate object WriteToSqlServerAction(ActionDO curActionDO);

        public const string Version = "1.0";

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
        public string WriteToSqlServer(ActionDTO curActionDTO, string path) {
            var curActionDO = Mapper.Map<ActionDO>(curActionDTO);
            var action = GetWriteToSqlServerAction(path);
            return JsonConvert.SerializeObject(action(curActionDO));
        }

        private WriteToSqlServerAction GetWriteToSqlServerAction(string path) {
            switch (path) {
                case "execute":         return Execute;
                case "field_mappings":  return GetFieldMappings;
                default:                return null;
            }
        }





        private const string C_FIELDMAPPINGS_QUERY = @"SELECT CONCAT('[', tbls.name, '].', cols.COLUMN_NAME) as tblcols" +
                                                     @"FROM sys.Tables tbls, INFORMATION_SCHEMA.COLUMNS cols" +
                                                     @"ORDER BY tbls.name, cols.COLUMN_NAME";

        [HttpPost]
        [Route("write_to_sql_server/field_mappings")]
        public object GetFieldMappings(ActionDO curActionDO) {             
            //Get configuration settings and check for connection string
            var settings = JsonConvert.DeserializeObject<JObject>(curActionDO.ConfigurationSettings);
            var connString = settings.Value<string>("connection_string");

            var curProvider = ObjectFactory.GetInstance<IDbProvider>();         

            return curProvider.ConnectToSql(connString, (command) => {
                command.CommandText = C_FIELDMAPPINGS_QUERY;

                //The list to serialize
                List<string> tableMetaData = new List<string>();

                //Iterate through each entry from the const query
                using (IDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        tableMetaData.Add(reader["tblcols"].ToString());
            }
                }

                //Serialize and return
                return tableMetaData;
            });
            }

        //TODO - SF - Not sure how to handle this method since the Command stuff is supposedly outdated
        //The original method still exists at the top of the plugin code
        [HttpPost]
        [Route("write_to_sql_server/execute")]
        private string Execute(ActionDO curActionDO) {
            return string.Empty;
            //try {
            //    // Creating ExtrationHelper and parsing WriteCommandArgs.
            //    var parser = new DbServiceJsonParser();
            //    var writeArgs = parser.ExtractWriteCommandArgs(curActionDO.);

            //    // Creating DbService and running WriteCommand logic.
            //    var dbService = new DbService();                
            //    dbService.WriteCommand(writeArgs);
            //}
            //catch (Exception ex) {
            //    return CommandResponse.ErrorResponse(ex.Message);
            //}
            //return CommandResponse.SuccessResponse();
        }

        [HttpGet]
        [Route("available")]
        public ActionTypeListDTO GetAvailable()
        {
            return new ActionTypeListDTO { TypeName = "write to azure sql server'", Version = "4.3" };
        }

        [HttpGet]
        [Route("configurationsettings")]
        public object GetConfigurationSettings()
        {
            return new { };
        }

        [HttpPost]
        [Route("field_mapping_targets")]
        public object GetFieldMappingTargets(ActionDTO curAction)
        {
            return new {};
        }
    }
}