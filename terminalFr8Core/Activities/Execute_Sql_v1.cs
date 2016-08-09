using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using terminalFr8Core.Infrastructure;
using TerminalSqlUtilities;

namespace terminalFr8Core.Activities
{
    public class Execute_Sql_v1 : ExplicitTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("23e0576e-7c51-42a6-89f2-e954c8499ca5"),
            Name = "Execute_Sql",
            Label = "Execute Sql Query",
            Version = "1",
            Tags = Tags.Internal,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Process,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;


        private const string DefaultDbProvider = "System.Data.SqlClient";

        #region Execution

        private QueryDTO ExtractSqlQuery()
        {
            var sqlQueryCrate = Payload.CratesOfType<StandardQueryCM>(x => x.Label == "Sql Query").FirstOrDefault();
            var queryCM = sqlQueryCrate?.Content;
            if (queryCM?.Queries == null || queryCM.Queries.Count == 0) { return null; }
            return queryCM.Queries[0];
        }

        private ColumnInfo CreateColumnInfo(string columnName, Dictionary<string, DbType> columnTypes)
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

        private StandardPayloadDataCM BuildStandardPayloadData(Table data, Dictionary<string, DbType> columnTypes)
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
                            new KeyValueDTO()
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
            var upstreamCrates = await HubCommunicator.GetCratesByDirection<KeyValueListCM>(ActivityId, CrateDirection.Upstream);
            var connectionStringCrate = upstreamCrates?.FirstOrDefault(x => x.Label == "Sql Connection String");
            var connectionStringCM = connectionStringCrate?.Content;
            var connectionStringFields = connectionStringCM?.Values;
            if (connectionStringFields == null || connectionStringFields.Count == 0) { return null; }
            return connectionStringFields[0].Key;
        }


        #endregion Execution

        public Execute_Sql_v1(ICrateManager crateManager)
            : base(crateManager)
        {
        }

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
            AddLabelControl("NoConfigLabel","No configuration","This activity does not require any configuration.");
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }
    }
}