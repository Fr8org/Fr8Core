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
        public const string Version = "1.0";
        public const string AvailableActions = "{'type_name':'write to azure sql server','version':4.3}";

        //private readonly IDbProvider _dbProvider;
        //private readonly JsonSerializer _serializer;

        //public ActionController(IDbProvider dbProvider, JsonSerializer serializer) {
        //    _dbProvider = dbProvider;
        //    _serializer = serializer;
        //}

        /// <summary>
        /// Insert user data to remote database tables.
        /// </summary>
        [HttpPost]
        [Route("writeSQL")]
        public CommandResponse Write(JObject data)
        {
            try
            {
                // Creating ExtrationHelper and parsing WriteCommandArgs.
                var parser = new DbServiceJsonParser();
                var writeArgs = parser.ExtractWriteCommandArgs(data);

                // Creating DbService and running WriteCommand logic.
                var dbService = new DbService();
                dbService.WriteCommand(writeArgs);
            }
            catch (Exception ex)
            {
                return CommandResponse.ErrorResponse(ex.Message);
            }

            return CommandResponse.SuccessResponse();
        }

        [HttpPost]
        [Route("write_to_sql_server/{path}")]
        public string WriteToSqlServer(ActionDTO actionDto, string path) {
            switch (path) {
                case "execute":
                    return JsonConvert.SerializeObject(Execute(actionDto));
                case "field_mappings":
                    return JsonConvert.SerializeObject(GetFieldMappings(actionDto));
                default:
                    return null;
            }

        }

        private const string C_FIELDMAPPINGS_QUERY = @"SELECT CONCAT('[', tbls.name, '].', cols.COLUMN_NAME) as tblcols" +
                                                     @"FROM sys.Tables tbls, INFORMATION_SCHEMA.COLUMNS cols" +
                                                     @"ORDER BY tbls.name, cols.COLUMN_NAME";

        [HttpPost]
        [Route("write_to_sql_server/field_mappings")]
        public IEnumerable<string> GetFieldMappings(ActionDTO actionDTO) {
            var curActionDO = Mapper.Map<ActionDO>(actionDTO);
 
            //Get configuration settings and check for connection string
            var settings = JsonConvert.DeserializeObject<JObject>(curActionDO.ConfigurationSettings);
            var connString = settings.Value<string>("connection_string");

            if (string.IsNullOrEmpty(connString)) {
                //Error point 1
                throw new PluginCodedException(PluginErrorCode.SQL_SERVER_CONNECTION_STRING_MISSING);
            }
            
            //We have a conn string, initiate db connection and open
            var dbProvider = ObjectFactory.GetInstance<SqlClientDbProvider>();
            var connection = dbProvider.CreateConnection(connString);

            try {
                //Open the connection, remember to close!!
                connection.Open();

                //Create a command from the const query to comb all tables/cols in one query
                var command = connection.CreateCommand();
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
            }
            catch (Exception ex) {
                //Should any exception be caught during the process, a connection failed error code is returned with the details
                throw new PluginCodedException(PluginErrorCode.SQL_SERVER_CONNECTION_FAILED, ex.Message);
            }
            finally {
                //Ensure the connection is closed after use if still open.
                if (connection.State != System.Data.ConnectionState.Closed) {
                    connection.Close();
                }
            }
        }
        
        //TODO - SF - Not sure how to handle this method since the Command stuff is supposedly outdated
        //The original method still exists at the top of the plugin code
        [HttpPost]
        [Route("write_to_sql_server/execute")]
        private string Execute(ActionDTO curActionDTO) {
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
        public string GetAvailable()
        {
            Validations.ValidateDtoString<ActionTypeListDTO>(AvailableActions);

            return AvailableActions;
        }

        [HttpGet]
        [Route("configurationsettings")]
        public string GetConfigurationSettings()
        {
            return string.Empty;
        }
    }
}