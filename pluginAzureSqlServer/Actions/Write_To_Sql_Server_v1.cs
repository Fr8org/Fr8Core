using System;
using System.Collections.Generic;
using System.Data;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pluginAzureSqlServer.Infrastructure;
using pluginAzureSqlServer.Services;
using PluginUtilities.Infrastructure;
using StructureMap;

namespace pluginAzureSqlServer.Actions {
    
    //Handler Action Delegates
    public delegate object WriteToSqlServerAction(ActionDO curActionDO);
    //Action container class
    public class Write_To_Sql_Server_v1 : ActionHandler {        

        //Public entry point, maps to actions from the controller
        public object Process(string path, ActionDO curActionDO) {
            switch (path) {
                case "execute":               return Execute(curActionDO);
                case "field_mappings":        return GetFieldMappings(curActionDO);
                case "configurationsettings": return GetConfigurationSettings(curActionDO);
                case "available":             return GetAvailable(curActionDO);
                default:                      return new {};
            }
        }

        public object Configure(ActionDO curActionDO)
        {
            return new ConfigurationSettingsDTO();
        }

        public object Activate(ActionDO curActionDO)
        {
            return null;
        }

        public object ExecuteV2(ActionDO curActionDO)
        {
            return null;
        }

        private const string ProviderName = "System.Data.SqlClient";
        private const string FieldMappingQuery = @"SELECT CONCAT('[', r.NAME, '].', r.COLUMN_NAME) as tblcols " +
                                                 @"FROM ( " +
	                                                @"SELECT DISTINCT tbls.NAME, cols.COLUMN_NAME " +
	                                                @"FROM sys.Tables tbls, INFORMATION_SCHEMA.COLUMNS cols " +
                                                 @") r " +
                                                 @"ORDER BY r.NAME, r.COLUMN_NAME";

        //[HttpPost]
        //[Route("write_to_sql_server/field_mappings")]
        private object GetFieldMappings(ActionDO curActionDO) {
            //Get configuration settings and check for connection string
            var settings = JsonConvert.DeserializeObject<JObject>(curActionDO.ConfigurationStore);
            var fieldsArray = settings.Value<JArray>("fields");

            string connString = null;
            foreach (var fieldObjectToken in fieldsArray)
            {
                var fieldObject = fieldObjectToken.ToObject<JObject>();
                if (fieldObject.Value<string>("name") != "connection_string")
                {
                    continue;
                }

                connString = fieldObject.Value<string>("value");
            }

            var curProvider = ObjectFactory.GetInstance<IDbProvider>();

            return curProvider.ConnectToSql(connString, (command) => {
                command.CommandText = FieldMappingQuery;

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

        //The original method still exists at the top of the plugin code
        //[HttpPost]
        //[Route("write_to_sql_server/execute")]
        private object Execute(ActionDO curActionDO)
        {
            var curCommandArgs = CreateCommandArgs(curActionDO);
            var dbService = new DbService();

            dbService.WriteCommand(curCommandArgs);

            return true;
        }

        private WriteCommandArgs CreateCommandArgs(ActionDO curActionDO)
        {
            var parser = new DbServiceJsonParser();
            var curConnStringObject = parser.ExtractConnectionString(curActionDO);
            var curCustomerData = ExtractCustomerData(curActionDO, parser);

            return new WriteCommandArgs(ProviderName, curConnStringObject, curCustomerData);
        }

        private IEnumerable<Table> ExtractCustomerData(ActionDO curActionDO,DbServiceJsonParser parser)
        {
            var payload = JsonConvert.DeserializeObject<JObject>(curActionDO.PayloadMappings);
            var payloadArray = payload.ExtractPropertyValue<JObject>("payload");
            
            if (payloadArray.Count == 0)
            {
                throw new Exception("\"payload\" data is empty");
            }

            var table = parser.CreateTable(payloadArray);

            return new List<Table> {table};
        }

        //[HttpGet]
        //[Route("available")]
        private static readonly ActionTypeListDTO AvailableActions = new ActionTypeListDTO {
            TypeName = "write to azure sql server",
            Version = "4.3"
        };
        private object GetAvailable(ActionDO curActionDO) {
            return AvailableActions;
        }

        //[HttpGet]
        //[Route("configurationsettings")]
        private object GetConfigurationSettings(ActionDO curActionDO) {
            return null;
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
                //var parser = new DbServiceJsonParser();
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

    }
}