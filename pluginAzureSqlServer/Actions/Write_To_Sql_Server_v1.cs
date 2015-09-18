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
using Core.Services;
using Core.StructureMap;
using Data.States.Templates;

namespace pluginAzureSqlServer.Actions
{

    public class Write_To_Sql_Server_v1 : BasePluginAction
    {

        private IAction _action;
        private ICrate _crate;

        public Write_To_Sql_Server_v1()
        {
            _action = ObjectFactory.GetInstance<IAction>();
            _crate = ObjectFactory.GetInstance<ICrate>();
        }




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

            //load configuration crates of manifest type Standard Control Crates
            //look for a text field name connection string with a value
            var controlsCrates = _action.GetCratesByManifestType(STANDARD_CONF_CONTROLS_NANIFEST_NAME,
                curActionDTO.CrateStorage);
            var connectionStringObjects = _crate.GetElementByKey(controlsCrates, key: "Connection String", keyFieldName: "key").ToArray();


            //if there are more than 2 return connection strings, something is wrong
            //if there are none or if there's one but it's value is "" the return initial else return followup
            var objCount = connectionStringObjects.Length;
            if (objCount > 1)
                throw new ArgumentException("didn't expect to see more than one connectionStringObject with the name Connection String on this Action", "curActionDTO");
            if (objCount == 0 || string.IsNullOrEmpty((string)connectionStringObjects.First()["value"]))
                return ConfigurationRequestType.Initial;
            else
            {
                return ConfigurationRequestType.Followup;
            }

            
        }

        //If the user provides no Connection String value, provide an empty Connection String field for the user to populate
        protected override CrateStorageDTO InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            // "[{ type: 'textField', name: 'connection_string', required: true, value: '', fieldLabel: 'SQL Connection String' }]"
            var fieldDefinitions = new List<FieldDefinitionDTO>() 
            {
                new FieldDefinitionDTO()
                {
                    FieldLabel = "SQL Connection String",
                    Type = "textField",
                    Name = "connection_string",
                    Required = true,
                    Events = new List<FieldEvent>() {new FieldEvent("onChange", "requestConfig")}
                },
                new FieldDefinitionDTO()
                {
                    FieldLabel = "SQL Connection String2",
                    Type = "textField",
                    Name = "connection_string2",
                    Required = true,
                    Events = new List<FieldEvent>() {new FieldEvent("onChange", "requestConfig")}
                }
            };

           
            //Return one field with empty connection string
            var curConfigurationStore = new CrateStorageDTO
            {
                //this needs to be updated to hold Crates instead of FieldDefinitionDTO
                CrateDTO = new List<CrateDTO>
                {
                    _crate.Create(
                        "AzureSqlServer Design-Time Fields",
                        JsonConvert.SerializeObject(fieldDefinitions),
                        "Standard Configuration Controls"
                        )
                }
            };

            return curConfigurationStore;
        }

        //if the user provides a connection string, this action attempts to connect to the sql server and get its columns and tables
        protected override CrateStorageDTO FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
            //In all followup calls, update data fields of the configuration store          
            List<String> contentsList = GetFieldMappings(curActionDTO);

            var curCrateStorageDTO = new CrateStorageDTO
            {
                //this needs to be updated to hold Crates instead of FieldDefinitionDTO
                CrateDTO = new List<CrateDTO>
                {
                    _crate.Create(
                        "Sql Table Columns",
                        JsonConvert.SerializeObject(contentsList),
                        "Standard Design-Time Fields"
                        )
                }
            };
           
            var curActionDO = AutoMapper.Mapper.Map<ActionDO>(curActionDTO);

            int foundSameCrateDTOAtIndex = curActionDO.CrateStorageDTO().CrateDTO.FindIndex(m => m.Label == "Sql Table Columns");
            if (foundSameCrateDTOAtIndex == -1)
            {
                _action.AddCrate(curActionDO, curCrateStorageDTO.CrateDTO.ToList());
            }
            else
            {
                CrateStorageDTO localList = curActionDO.CrateStorageDTO();
                localList.CrateDTO.RemoveAt(foundSameCrateDTOAtIndex);
                curActionDO.CrateStorage = JsonConvert.SerializeObject(localList);
                _action.AddCrate(curActionDO, curCrateStorageDTO.CrateDTO.ToList());
            }
            curCrateStorageDTO = curActionDO.CrateStorageDTO();

            return curCrateStorageDTO;
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

        public List<string> GetFieldMappings(ActionDTO curActionDTO)
        {
            //Get configuration settings and check for connection string
            CrateStorageDTO curCrates = curActionDTO.CrateStorage;
            if (curActionDTO.CrateStorage.CrateDTO.Count == 0)
            {
                throw new PluginCodedException(PluginErrorCode.SQL_SERVER_CONNECTION_STRING_MISSING);
            }

            var curConnectionStringFieldList =
                JsonConvert.DeserializeObject<List<FieldDefinitionDTO>>(curCrates.CrateDTO.First(field => field.Contents.Contains("connection_string")).Contents);

            if (curConnectionStringFieldList == null)
            {
                throw new PluginCodedException(PluginErrorCode.SQL_SERVER_CONNECTION_STRING_MISSING);
            }

            var connStringField = curConnectionStringFieldList.First();
            if (connStringField == null || String.IsNullOrEmpty(connStringField.Value))
            {
                throw new PluginCodedException(PluginErrorCode.SQL_SERVER_CONNECTION_STRING_MISSING);
            }

            var curProvider = ObjectFactory.GetInstance<IDbProvider>();

            return (List<string>)curProvider.ConnectToSql(connStringField.Value, (command) =>
            {
                command.CommandText = FieldMappingQuery;

                //The list to serialize
                List<string> tableMetaData = new List<string>();

                //Iterate through each entry from the const query
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
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

        private IEnumerable<Table> ConvertActionPayloadToSqlInputs(ActionDO curActionDO, DbServiceJsonParser parser)
        {
            //replace this with a Crate-based solution
            JObject payload = new JObject();//JsonConvert.DeserializeObject<JObject>(curActionDO.PayloadMappings);
            var payloadArray = payload.ExtractPropertyValue<JObject>("payload");

            if (payloadArray.Count == 0)
            {
                throw new Exception("\"payload\" data is empty");
            }

            var table = parser.CreateTable(payloadArray);

            return new List<Table> { table };
        }
    }
}