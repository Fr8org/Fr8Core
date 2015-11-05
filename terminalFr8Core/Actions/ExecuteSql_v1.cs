using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Enums;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using TerminalSqlUtilities;
using terminalFr8Core.Infrastructure;

namespace terminalFr8Core.Actions
{
    public class ExecuteSql_v1 : BasePluginAction
    {
        private const string DefaultDbProvider = "System.Data.SqlClient";


        #region Configuration

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO)
        {
            return ConfigurationRequestType.Initial;
        }

        protected override Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            AddLabelControl(
                curActionDTO,
                "NoConfigLabel",
                "No configuration",
                "This action does not require any configuration."
            );

            return Task.FromResult(curActionDTO);
        }

        #endregion Configuration

        #region Execution

        private QueryDTO ExtractSqlQuery(PayloadDTO payload)
        {
            var sqlQueryCrate = payload.CrateStorageDTO().CrateDTO.FirstOrDefault(
                x => x.Label == "Sql Query"
                    && x.ManifestType == CrateManifests.STANDARD_PAYLOAD_MANIFEST_NAME);

            if (sqlQueryCrate == null) { return null; }

            
            var queryCM = JsonConvert.DeserializeObject<StandardQueryCM>(sqlQueryCrate.Contents);
            if (queryCM == null || queryCM.Queries == null || queryCM.Queries.Count == 0) { return null; }

            return queryCM.Queries[0];
        }

        private ColumnInfo CreateColumnInfo(
            string columnName, Dictionary<string, DbType> columnTypes)
        {
            DbType dbType;
            if (!columnTypes.TryGetValue(columnName, out dbType))
            {
                throw new ApplicationException("No column db-type found.");
            }

            var columnInfo = new ColumnInfo(columnName, dbType);
            return columnInfo;
        }

        private SelectQuery BuildQuery(string connectionString,
            QueryDTO query, Dictionary<string, DbType> columnTypes)
        {
            var tableInfo = new TableInfo(query.Name);
            var tableColumns = columnTypes.Keys.Where(x => x.StartsWith(query.Name)).ToList();

            var columns = tableColumns.Select(x => CreateColumnInfo(x, columnTypes)).ToList();

            var returnedQuery = new SelectQuery(connectionString, tableInfo, columns, query.Criteria);
            return returnedQuery;
        }

        private StandardPayloadDataCM BuildStandardPayloadData(
            Table data, Dictionary<string, DbType> columnTypes)
        {
            var findObjectHelper = new FindObjectHelper();

            var payloadCM = new StandardPayloadDataCM();

            var tableData = new List<PayloadObjectDTO>();
            
            if (data.Rows != null)
            {
                foreach (var row in data.Rows)
                {
                    if (row.Values == null) { continue; }

                    var payloadObject = new PayloadObjectDTO();
                    foreach (var fieldValue in row.Values)
                    {
                        var columnName = data.TableInfo.ToString() + "." + fieldValue.Field;

                        DbType dbType;
                        if (!columnTypes.TryGetValue(columnName, out dbType))
                        {
                            throw new ApplicationException("No column data type found.");
                        }

                        payloadObject.PayloadObject.Add(
                            new FieldDTO()
                            {
                                Key = fieldValue.Field,
                                Value = findObjectHelper.ConvertValueToString(fieldValue.Value, dbType)
                            }
                        );
                    }
                }
            }

            payloadCM.PayloadObjects.AddRange(tableData);

            return payloadCM;
        }

        private async Task<string> ExtractConnectionString(ActionDTO actionDTO)
        {
            var upstreamCrates = await GetCratesByDirection(
                actionDTO.Id,
                CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME,
                GetCrateDirection.Upstream
            );

            if (upstreamCrates == null) { return null; }

            var connectionStringCrate = upstreamCrates
                .FirstOrDefault(x => x.Label == "Sql Connection String");

            if (connectionStringCrate == null) { return null; }

            var connectionStringCM = JsonConvert
                .DeserializeObject<StandardDesignTimeFieldsCM>(connectionStringCrate.Contents);

            if (connectionStringCM == null) { return null; }

            var connectionStringFields = connectionStringCM.Fields;
            if (connectionStringFields == null || connectionStringFields.Count == 0) { return null; }

            return connectionStringFields[0].Key;
        }

        public async Task<PayloadDTO> Run(ActionDTO curActionDTO)
        {
            var findObjectHelper = new FindObjectHelper();
            var payload = await GetProcessPayload(curActionDTO.ProcessId);

            var columnTypes = await findObjectHelper.ExtractColumnTypes(this, curActionDTO);
            if (columnTypes == null)
            {
                throw new ApplicationException("No column types crate found.");
            }

            var queryPayloadValue = ExtractSqlQuery(payload);
            if (queryPayloadValue == null)
            {
                throw new ApplicationException("No Sql Query payload crate found.");
            }

            var connectionString = await ExtractConnectionString(curActionDTO);

            var query = BuildQuery(connectionString, queryPayloadValue, columnTypes);

            var dbProvider = DbProvider.GetDbProvider(DefaultDbProvider);
            var data = dbProvider.ExecuteQuery(query);
            var payloadCM = BuildStandardPayloadData(data, columnTypes);

            var payloadCMCrate = Crate.Create(
                "Sql Query Result",
                JsonConvert.SerializeObject(payloadCM),
                CrateManifests.STANDARD_PAYLOAD_MANIFEST_NAME,
                CrateManifests.STANDARD_PAYLOAD_MANIFEST_ID
            );

            payload.UpdateCrateStorageDTO(new List<CrateDTO>() { payloadCMCrate });

            return payload;
        }

        #endregion Execution
    }
}