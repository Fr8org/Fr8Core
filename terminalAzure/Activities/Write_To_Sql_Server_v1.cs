using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using StructureMap;
using terminalAzure.Infrastructure;
using terminalAzure.Services;
using TerminalBase;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using TerminalSqlUtilities;

namespace terminalAzure.Actions
{

    public class Write_To_Sql_Server_v1 : BaseTerminalActivity
    {

        //================================================================================
        //General Methods (every Action class has these)

        //maybe want to return the full Action here
        public override async Task<ActivityDTO> Configure(ActivityDTO curActivityDTO, AuthorizationTokenDTO authTokenDTO)
        {
            return await ProcessConfigurationRequest(curActivityDTO, EvaluateReceivedRequest, authTokenDTO);
        }

        //this entire function gets passed as a delegate to the main processing code in the base class
        //currently many actions have two stages of configuration, and this method determines which stage should be applied
        private ConfigurationRequestType EvaluateReceivedRequest(ActivityDTO curActivityDTO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDTO))
                return ConfigurationRequestType.Initial;

            //load configuration crates of manifest type Standard Control Crates
            //look for a text field name connection string with a value

            var storage = CrateManager.GetStorage(curActivityDTO);

            var connectionStrings = storage.CratesOfType<StandardConfigurationControlsCM>()
                .Select(x => x.Content.FindByName("connection_string"))
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.Value))
                .ToArray();

            //if there are more than 2 return connection strings, something is wrong
            //if there are none or if there's one but it's value is "" the return initial else return followup
            var objCount = connectionStrings.Length;
            if (objCount > 1)
                throw new ArgumentException("didn't expect to see more than one connectionStringObject with the name Connection String on this Action", "curActivityDO");
            if (objCount == 0)
                return ConfigurationRequestType.Initial;
            else
            {
                //we should validate our data now
                //CheckFields(curActivityDO, new List<ValidationDataTuple> { new ValidationDataTuple("connection_string", "test", GetCrateDirection.Upstream, CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME) });
                return ConfigurationRequestType.Followup;
            }
        }

        //If the user provides no Connection String value, provide an empty Connection String field for the user to populate
        protected override async Task<ActivityDTO> InitialConfigurationResponse(ActivityDTO curActivityDTO, AuthorizationTokenDTO authTokenDTO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDTO))
            {
                crateStorage.Clear();
                crateStorage.Add(CreateControlsCrate());
            }

            return await Task.FromResult<ActivityDTO>(curActivityDTO);
        }

        private Crate CreateControlsCrate()
        {
            // "[{ type: 'textField', name: 'connection_string', required: true, value: '', fieldLabel: 'SQL Connection String' }]"
            var controls = new ControlDefinitionDTO[]{
                new TextBox
                {
                    Label = "SQL Connection String",
                    Name = "connection_string",
                    Required = true
                },
                new Button
                {
                    Label = "Continue",
                    Name = "Continue",
                    Events = new List<ControlEvent>()
                    {
                        new ControlEvent("onClick", "requestConfig")
                    }
                }
            };
            return PackControlsCrate(controls);
        }

        //if the user provides a connection string, this action attempts to connect to the sql server and get its columns and tables
        protected override async Task<ActivityDTO> FollowupConfigurationResponse(ActivityDTO curActivityDO, AuthorizationTokenDTO authTokenDO)
        {
            //Verify controls, make sure that TextBox with value exists
            ValidateControls(curActivityDO);
            //In all followup calls, update data fields of the configuration store          
            List<String> contentsList;
            try
            {
                contentsList = GetFieldMappings(curActivityDO);
                using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
                {
                    crateStorage.RemoveByLabel("Sql Table Columns");
                    //this needs to be updated to hold Crates instead of FieldDefinitionDTO
                    crateStorage.Add(CrateManager.CreateDesignTimeFieldsCrate("Sql Table Columns", contentsList.Select(col => new FieldDTO() { Key = col, Value = col }).ToArray()));
                }
            }
            catch
            {
                AddErrorToControl(curActivityDO);
            }
            return await Task.FromResult<ActivityDTO>(curActivityDO);
        }
        public async Task<PayloadDTO> Run(ActivityDTO activityDO, Guid containerId, AuthorizationTokenDTO authTokenDO)
        {
            var payloadCrates = await GetPayload(activityDO, containerId);

            var curCommandArgs = PrepareSQLWrite(activityDO, payloadCrates);

            var dbService = new DbService();
            dbService.WriteCommand(curCommandArgs);

            return Success(payloadCrates);
        }

        //===============================================================================================
        //Specialized Methods (Only found in this Action class)

        private const string ProviderName = "System.Data.SqlClient";
        private const string FieldMappingQuery = @"SELECT CONCAT('[', r.TABLE_NAME, '].', r.COLUMN_NAME) as tblcols " +
                                                 @"FROM ( " +
                                                    @"SELECT DISTINCT tbls.TABLE_NAME, cols.COLUMN_NAME " +
                                                    @"FROM INFORMATION_SCHEMA.Tables tbls INNER JOIN INFORMATION_SCHEMA.COLUMNS cols ON tbls.TABLE_NAME = cols.TABLE_NAME " +
                                                 @") r " +
                                                 @"ORDER BY r.TABLE_NAME, r.COLUMN_NAME";


        //CONFIGURATION-Related Methods
        //-----------------------------------------

        public List<string> GetFieldMappings(ActivityDTO curActivityDO)
        {
            var confControls = GetConfigurationControls(curActivityDO);
            var connStringField = confControls.Controls.First();
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

        private void ValidateControls(ActivityDTO activityDO)
        {
            var storage = CrateManager.GetStorage(activityDO);

            if (storage.Count == 0)
            {
                throw new TerminalCodedException(TerminalErrorCode.SQL_SERVER_CONNECTION_STRING_MISSING);
            }

            var confControls = storage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (confControls == null)
            {
                throw new TerminalCodedException(TerminalErrorCode.SQL_SERVER_CONNECTION_STRING_MISSING);
            }

            var connStringField = confControls.Controls.First();
            if (connStringField == null || String.IsNullOrEmpty(connStringField.Value))
            {
                throw new TerminalCodedException(TerminalErrorCode.SQL_SERVER_CONNECTION_STRING_MISSING);
            }
        }
        private void AddErrorToControl(ActivityDTO activityDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(activityDO))
            {
                var controls = GetConfigurationControls(crateStorage);
                var connStringTextBox = GetControl(controls, "connection_string", ControlTypes.TextBox);
                connStringTextBox.Value = "Incorrect Connection String";
            }
        }

        //EXECUTION-Related Methods
        //-----------------------------------------
        private WriteCommandArgs PrepareSQLWrite(ActivityDTO curActivityDO, PayloadDTO payloadCrates)
        {
            var parser = new DbServiceJsonParser();
            var curConnStringObject = parser.ExtractConnectionString(curActivityDO);
            var curSQLData = ConvertProcessPayloadToSqlInputs(payloadCrates);

            return new WriteCommandArgs(ProviderName, curConnStringObject, curSQLData);
        }

        private IEnumerable<Table> ConvertProcessPayloadToSqlInputs(PayloadDTO payloadCrates)
        {
            var mappedFieldsCrate = CrateManager.GetStorage(payloadCrates).CratesOfType<StandardPayloadDataCM>().FirstOrDefault(x => x.Label == "MappedFields");

            if (mappedFieldsCrate == null)
            {
                throw new ApplicationException("No payload crate found with Label == MappedFields.");
            }

            var valuesCrate = CrateManager.GetStorage(payloadCrates).CratesOfType<StandardPayloadDataCM>().FirstOrDefault(x => x.Label == "TableData");

            if (valuesCrate == null)
            {
                throw new ApplicationException("No payload crate found with Label == TableData");
            }
            return CreateTables(mappedFieldsCrate.Content.AllValues().ToList(), valuesCrate.Content.AllValues().ToList());
        }

        private IEnumerable<Table> CreateTables(List<FieldDTO> fields, List<FieldDTO> values)
        {
            // Map tableName -> field -> value.
            var tableMap = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, string> columnMap = new Dictionary<string, string>();

            foreach (var field in fields)
            {
                var tableTokens = field.Value.Split('.');
                if (tableTokens == null || tableTokens.Length != 2)
                {
                    throw new ApplicationException(string.Format("Invalid column name {0}", field.Value));
                }

                var tableName = PrepareSqlName(tableTokens[0]);
                var columnName = PrepareSqlName(tableTokens[1]);
                var valueField = values.FirstOrDefault(x => x.Key == field.Key);

                string value = "";
                if (valueField != null)
                {
                    value = valueField.Value;
                }

                // Insert fields into table
                columnMap[columnName] = value;
                // Insert table into table dictionary
                tableMap[tableName] = columnMap;
            }

            var tables = CreateTablesFromMap(tableMap);
            return tables;
        }

        private IEnumerable<Table> CreateTablesFromMap(
            Dictionary<string, Dictionary<string, string>> tableMap)
        {
            var tables = new List<Table>();

            foreach (var tablePair in tableMap)
            {
                var values = new List<FieldValue>();

                foreach (var columnPair in tablePair.Value)
                {
                    values.Add(new FieldValue(columnPair.Key, columnPair.Value));
                }

                // TODO: change "dbo" schema later.
                tables.Add(new Table(new TableInfo("dbo", tablePair.Key), new[] { new Row(values) }));
            }

            return tables;
        }

        private string PrepareSqlName(string rawName)
        {
            return rawName.Replace("[", "").Replace("]", "");
        }
    }
}