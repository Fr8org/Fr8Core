using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using TerminalSqlUtilities;
using terminalFr8Core.Infrastructure;
using Data.Entities;
using Data.States;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;

namespace terminalFr8Core.Actions
{
    public class ExecuteSql_v1 : BaseTerminalActivity
    {
        private const string DefaultDbProvider = "System.Data.SqlClient";

        #region Execution

        private QueryDTO ExtractSqlQuery()
        {
            var sqlQueryCrate = Payload.CratesOfType<StandardQueryCM>(x => x.Label == "Sql Query").FirstOrDefault();
            var queryCM = sqlQueryCrate?.Content;
            if (queryCM?.Queries == null || queryCM.Queries.Count == 0) { return null; }
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

                    tableData.Add(payloadObject);
                }
            }

            payloadCM.PayloadObjects.AddRange(tableData);

            return payloadCM;
        }

        private async Task<string> ExtractConnectionString()
        {
            var upstreamCrates = await GetCratesByDirection<FieldDescriptionsCM>(CrateDirection.Upstream);
            var connectionStringCrate = upstreamCrates?.FirstOrDefault(x => x.Label == "Sql Connection String");
            var connectionStringCM = connectionStringCrate?.Content;
            var connectionStringFields = connectionStringCM?.Fields;
            if (connectionStringFields == null || connectionStringFields.Count == 0) { return null; }
            return connectionStringFields[0].Key;
        }


        #endregion Execution

        public ExecuteSql_v1() : base(false)
        {
        }

        protected override ActivityTemplateDTO MyTemplate { get; }
        public override async Task Run()
        {
            var findObjectHelper = new FindObjectHelper();
            var columnTypes = await findObjectHelper.ExtractColumnTypes(HubCommunicator, ActivityContext);
            if (columnTypes == null)
            {
                RaiseError("No column types crate found.");
                return;
            }

            var queryPayloadValue = ExtractSqlQuery();
            if (queryPayloadValue == null)
            {
                RaiseError("No Sql Query payload crate found.");
                return;
            }

            var connectionString = await ExtractConnectionString();

            var query = BuildQuery(connectionString, queryPayloadValue, columnTypes);

            var dbProvider = DbProvider.GetDbProvider(DefaultDbProvider);
            var data = dbProvider.ExecuteQuery(query);
            var payloadCM = BuildStandardPayloadData(data, columnTypes);
            var payloadCMCrate = Crate.FromContent("Sql Query Result", payloadCM);
            Payload.Add(payloadCMCrate);
            Success();
        }

        public override async Task Initialize()
        {
            Storage.Add(PackControlsCrate());
            AddLabelControl("NoConfigLabel","No configuration","This activity does not require any configuration.");
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }
    }
}