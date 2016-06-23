using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using TerminalSqlUtilities;

namespace terminalFr8Core.Infrastructure
{
    public class FindObjectHelper
    {
        private const string DefaultDbProvider = "System.Data.SqlClient";

        public async Task<Dictionary<string, DbType>> ExtractColumnTypes(
            IHubCommunicator hubCommunicator, ActivityContext activityContext)
        {
            var upstreamCrates = await hubCommunicator.GetCratesByDirection<KeyValueListCM>(activityContext.ActivityPayload.Id, CrateDirection.Upstream);

            if (upstreamCrates == null) { return null; }

            var columnTypesCrate = upstreamCrates
                .FirstOrDefault(x => x.Label == "Sql Column Types");

            if (columnTypesCrate == null) { return null; }

            var columnTypes = columnTypesCrate.Content;

            if (columnTypes == null) { return null; }

            var columnTypeFields = columnTypes.Values;
            if (columnTypeFields == null) { return null; }

            var columnTypeMap = GetColumnTypeMap(columnTypeFields);
            return columnTypeMap;
        }

        public string ConvertValueToString(object value, DbType dbType)
        {
            return Convert.ToString(value);
        }

        private void ListAllDbColumns(string connectionString, Action<IEnumerable<ColumnInfo>> callback)
        {
            var provider = DbProvider.GetDbProvider(DefaultDbProvider);

            using (var conn = provider.CreateConnection(connectionString))
            {
                conn.Open();

                using (var tx = conn.BeginTransaction())
                {
                    var columns = provider.ListAllColumns(tx);

                    if (callback != null)
                    {
                        callback.Invoke(columns);
                    }
                }
            }
        }

        public List<FieldDTO> RetrieveTableDefinitions(string connectionString)
        {
            var tableNames = new HashSet<string>();

            ListAllDbColumns(connectionString, columns =>
            {
                foreach (var column in columns)
                {
                    tableNames.Add(column.TableInfo.ToString());
                }
            });

            var fieldsList = tableNames
                .Select(x => new FieldDTO(x))
                .OrderBy(x => x.Name)
                .ToList();

            return fieldsList;
        }

        public List<KeyValueDTO> RetrieveColumnDefinitions(string connectionString)
        {
            var fieldsList = new List<KeyValueDTO>();

            ListAllDbColumns(connectionString, columns =>
            {
                foreach (var column in columns)
                {
                    var fullColumnName = column.ToString();

                    fieldsList.Add(new KeyValueDTO()
                    {
                        Key = fullColumnName,
                        Value = fullColumnName
                    });
                }
            });

            return fieldsList;
        }

        public List<KeyValueDTO> RetrieveColumnTypes(string connectionString)
        {
            var fieldsList = new List<KeyValueDTO>();

            ListAllDbColumns(connectionString, columns =>
            {
                foreach (var column in columns)
                {
                    var fullColumnName = column.ToString();

                    fieldsList.Add(new KeyValueDTO()
                    {
                        Key = fullColumnName,
                        Value = column.DbType.ToString()
                    });
                }
            });

            return fieldsList;
        }

        public Dictionary<string, DbType> GetColumnTypeMap(List<KeyValueDTO> columnTypeFields)
        {
            var columnTypeMap = new Dictionary<string, DbType>();
            foreach (var columnType in columnTypeFields)
            {
                columnTypeMap.Add(columnType.Key, (DbType)Enum.Parse(typeof(DbType), columnType.Value));
            }

            return columnTypeMap;
        }

        public List<KeyValueDTO> MatchColumnsForSelectedObject(string connectionString, string selectedObject)
        {
            var columnDefinitions = RetrieveColumnDefinitions(connectionString);
            var columnTypes = RetrieveColumnTypes(connectionString);
            var columnTypeMap = GetColumnTypeMap(columnTypes);

            var result = MatchColumnsForSelectedObject(
                columnDefinitions, selectedObject, columnTypeMap);

            return result;
        }

        public List<KeyValueDTO> MatchColumnsForSelectedObject(IEnumerable<KeyValueDTO> columnDefinitions,
            string selectedObject, IDictionary<string, DbType> columnTypeMap)
        {
            if (columnDefinitions == null || columnTypeMap == null)
            {
                columnDefinitions = new List<KeyValueDTO>();
            }

            var supportedColumnTypes = new HashSet<DbType>() { DbType.String, DbType.Int32, DbType.Boolean };

            // Match columns and filter by supported column type.
            List<KeyValueDTO> matchedColumns;
            if (string.IsNullOrEmpty(selectedObject))
            {
                matchedColumns = new List<KeyValueDTO>();
            }
            else
            {
                matchedColumns = columnDefinitions
                    .Where(x => x.Key.StartsWith(selectedObject))
                    .Where(x => supportedColumnTypes.Contains(columnTypeMap[x.Key]))
                    .Select(x =>
                    {
                        var tokens = x.Key.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                        var columnName = tokens[tokens.Length - 1];

                        return new KeyValueDTO() { Key = columnName, Value = columnName };
                    })
                    .ToList();
            }

            return matchedColumns;
        }
    }
}