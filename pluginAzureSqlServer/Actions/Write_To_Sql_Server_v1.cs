using System.Collections.Generic;
using System.Data;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pluginAzureSqlServer.Infrastructure;
using PluginUtilities.Infrastructure;
using StructureMap;

namespace pluginAzureSqlServer.Actions {
    
    //Handler Action Delegates
    public delegate object WriteToSqlServerAction(ActionDO curActionDO);

    //Action container class
    public class Write_To_Sql_Server_v1 : ActionHandler {        

        //Public entry point, maps to actions from the controller
        public object WriteToSqlServerAction(string path, ActionDO curActionDO) {
            switch (path) {
                case "execute":               return Execute(curActionDO);
                case "field_mappings":        return GetFieldMappings(curActionDO);
                case "configurationsettings": return GetConfigurationSettings(curActionDO);
                case "available":             return GetAvailable(curActionDO);
                default:                      return new {};
            }
        }

               
        private const string FieldMappingQuery = @"SELECT CONCAT('[', tbls.name, '].', cols.COLUMN_NAME) as tblcols" +
                                                 @"FROM sys.Tables tbls, INFORMATION_SCHEMA.COLUMNS cols" +
                                                 @"ORDER BY tbls.name, cols.COLUMN_NAME";
        //[HttpPost]
        //[Route("write_to_sql_server/field_mappings")]
        private object GetFieldMappings(ActionDO curActionDO) {
            //Get configuration settings and check for connection string
            var settings = JsonConvert.DeserializeObject<JObject>(curActionDO.ConfigurationSettings);
            var connString = settings.Value<string>("connection_string");

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

        //TODO - SF - Not sure how to handle this method since the Command stuff is supposedly outdated
        //The original method still exists at the top of the plugin code
        //[HttpPost]
        //[Route("write_to_sql_server/execute")]
        private object Execute(ActionDO curActionDO) {
            return null;
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
    }
}