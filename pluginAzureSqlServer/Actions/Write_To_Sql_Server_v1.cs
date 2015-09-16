using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pluginAzureSqlServer.Infrastructure;
using pluginAzureSqlServer.Services;
using PluginBase.Infrastructure;
using StructureMap;
using PluginBase;
using PluginBase.BaseClasses;
using Core.Interfaces;

namespace pluginAzureSqlServer.Actions {
    
    public class Write_To_Sql_Server_v1 : BasePluginAction {        

       //================================================================================
       //General Methods (every Action class has these)

        //maybe want to return the full Action here
        public CrateStorageDTO Configure(ActionDTO curActionDTO)
        {
            return ProcessConfigurationRequest(curActionDTO, EvaluateReceivedRequest);
        }

        //this entire function gets passed as a delegate to the main processing code in the base class
        //currently many actions have two stages of configuration, and this method determines which stage should be applied
        private ConfigurationRequestType EvaluateReceivedRequest(ActionDTO curActionDTO)
        {
            CrateStorageDTO curCrates = curActionDTO.CrateStorage;

            if (curCrates.CrateDTO.Count == 0)
                return ConfigurationRequestType.Initial;

            var curConnectionStringField =
                JsonConvert.DeserializeObject<FieldDefinitionDTO>(curCrates.CrateDTO.First(field => field.Contents.Contains("connection_string")).Contents);

            if (curConnectionStringField != null)
            {
                if (string.IsNullOrEmpty(curConnectionStringField.Value))
                {
                    //Scenario 1 - This is the first request being made by this Action
                    //Return Initial configuration request type
                    return ConfigurationRequestType.Initial;
                }
                else
                {
                    //This else block covers 2nd and 3rd scenarios as mentioned below
                    //Scenario 2 - This is the seond request, being made after the user filled in the value of the connection string
                    //Scenario 3 - A data_fields was previously constructed, but perhaps the connection string has changed.
                    //in either scenario, we have to return Followup configuration request type
                    return ConfigurationRequestType.Followup;
                }
            }
            else
            {
                throw new ApplicationException("this value should never be null");
            }
        }

        //If the user provides no Connection String value, provide an empty Connection String field for the user to populate
        protected override CrateStorageDTO InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            ICrate _crate = ObjectFactory.GetInstance<ICrate>();
            //Return one field with empty connection string
            CrateStorageDTO curConfigurationStore = new CrateStorageDTO
            {

                //this needs to be updated to hold Crates instead of FieldDefinitionDTO
                CrateDTO = new List<CrateDTO>
                {
                    _crate.Create("AzureSqlServer Design-Time Fields", "{ type: 'textField', name: 'connection_string', required: true, value: '', fieldLabel: 'SQL Connection String' }", "Standard Configuration Controls")
                }
            };

            return curConfigurationStore;
        }

        //if the user provides a connection string, this action attempts to connect to the sql server and get its columns and tables
        protected override CrateStorageDTO FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
            //In all followup calls, update data fields of the configuration store
            CrateStorageDTO curConfigurationStore = curActionDTO.CrateStorage;

            return curConfigurationStore;
        }

        public object Activate(ActionDO curActionDO)
        {
            //not currently any requirements that need attention at Activation Time
            return null;
        }

        public object Execute(ActionDataPackageDTO curActionDataPackage)
        {
            var curActionDO = AutoMapper.Mapper.Map<ActionDO>(curActionDataPackage.ActionDTO);
            var curCommandArgs = PrepareSQLWrite(curActionDO);
            var dbService = new DbService();

            dbService.WriteCommand(curCommandArgs);

            return true;
        }

      //===============================================================================================
      //Specialized Methods (Only found in this Action class)

        private const string ProviderName = "System.Data.SqlClient";
        private const string FieldMappingQuery = @"SELECT CONCAT('[', r.NAME, '].', r.COLUMN_NAME) as tblcols " +
                                                 @"FROM ( " +
	                                                @"SELECT DISTINCT tbls.NAME, cols.COLUMN_NAME " +
	                                                @"FROM sys.Tables tbls, INFORMATION_SCHEMA.COLUMNS cols " +
                                                 @") r " +
                                                 @"ORDER BY r.NAME, r.COLUMN_NAME";


        //CONFIGURATION-Related Methods
        //-----------------------------------------
     
        public object GetFieldMappings(ActionDO curActionDO) {
            //Get configuration settings and check for connection string
            if (string.IsNullOrEmpty(curActionDO.CrateStorage))
            {
                throw new PluginCodedException(PluginErrorCode.SQL_SERVER_CONNECTION_STRING_MISSING);
            }

            var configuration = JsonConvert.DeserializeObject<FieldDefinitionDTO>(curActionDO.CrateStorageDTO().CrateDTO.First().Contents);
            if (configuration == null || curActionDO.CrateStorageDTO().CrateDTO.Count == 0)
                {
                throw new PluginCodedException(PluginErrorCode.SQL_SERVER_CONNECTION_STRING_MISSING);
                }

            var connStringField = configuration;
            if (connStringField == null || String.IsNullOrEmpty(connStringField.Value))
            {
                throw new PluginCodedException(PluginErrorCode.SQL_SERVER_CONNECTION_STRING_MISSING);
            }
            string connString = connStringField.Value;

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


        //EXECUTION-Related Methods
        //-----------------------------------------
        private WriteCommandArgs PrepareSQLWrite(ActionDO curActionDO)
        {
            var parser = new DbServiceJsonParser();
            var curConnStringObject = parser.ExtractConnectionString(curActionDO);
            var curSQLData = ConvertActionPayloadToSqlInputs(curActionDO, parser);

            return new WriteCommandArgs(ProviderName, curConnStringObject, curSQLData);
        }



        private IEnumerable<Table> ConvertActionPayloadToSqlInputs(ActionDO curActionDO,DbServiceJsonParser parser)
        {
            //replace this with a Crate-based solution
            JObject payload = new JObject();//JsonConvert.DeserializeObject<JObject>(curActionDO.PayloadMappings);
            var payloadArray = payload.ExtractPropertyValue<JObject>("payload");
            
            if (payloadArray.Count == 0)
            {
                throw new Exception("\"payload\" data is empty");
            }

            var table = parser.CreateTable(payloadArray);

            return new List<Table> {table};
        }



    }
}