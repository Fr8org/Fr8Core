using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.States;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using TerminalSqlUtilities;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalFr8Core.Infrastructure
{
    public class FindObjectHelper
    {
        private const string DefaultDbProvider = "System.Data.SqlClient";

        public async Task<Dictionary<string, DbType>> ExtractColumnTypes(
            IHubCommunicator hubCommunicator, ActivityDO activityDO)
        {
            var upstreamCrates = await hubCommunicator.GetCratesByDirection<FieldDescriptionsCM>(
                activityDO,
                CrateDirection.Upstream
            );

            if (upstreamCrates == null) { return null; }

            var columnTypesCrate = upstreamCrates
                .FirstOrDefault(x => x.Label == "Sql Column Types");

            if (columnTypesCrate == null) { return null; }

            var columnTypes = columnTypesCrate.Content;

            if (columnTypes == null) { return null; }

            var columnTypeFields = columnTypes.Fields;
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
                .Select(x => new FieldDTO(x, x))
                .OrderBy(x => x.Key)
                .ToList();

            return fieldsList;
        }

        public List<FieldDTO> RetrieveColumnDefinitions(string connectionString)
        {
            var fieldsList = new List<FieldDTO>();

            ListAllDbColumns(connectionString, columns =>
            {
                foreach (var column in columns)
                {
                    var fullColumnName = column.ToString();

                    fieldsList.Add(new FieldDTO()
                    {
                        Key = fullColumnName,
                        Value = fullColumnName
                    });
                }
            });

            return fieldsList;
        }

        public List<FieldDTO> RetrieveColumnTypes(string connectionString)
        {
            var fieldsList = new List<FieldDTO>();

            ListAllDbColumns(connectionString, columns =>
            {
                foreach (var column in columns)
                {
                    var fullColumnName = column.ToString();

                    fieldsList.Add(new FieldDTO()
                    {
                        Key = fullColumnName,
                        Value = column.DbType.ToString()
                    });
                }
            });

            return fieldsList;
        }

        public Dictionary<string, DbType> GetColumnTypeMap(List<FieldDTO> columnTypeFields)
        {
            var columnTypeMap = new Dictionary<string, DbType>();
            foreach (var columnType in columnTypeFields)
            {
                columnTypeMap.Add(columnType.Key, (DbType)Enum.Parse(typeof(DbType), columnType.Value));
            }

            return columnTypeMap;
        }

        public List<FieldDTO> MatchColumnsForSelectedObject(string connectionString, string selectedObject)
        {
            var columnDefinitions = RetrieveColumnDefinitions(connectionString);
            var columnTypes = RetrieveColumnTypes(connectionString);
            var columnTypeMap = GetColumnTypeMap(columnTypes);

            var result = MatchColumnsForSelectedObject(
                columnDefinitions, selectedObject, columnTypeMap);

            return result;
        }

        public List<FieldDTO> MatchColumnsForSelectedObject(IEnumerable<FieldDTO> columnDefinitions,
            string selectedObject, IDictionary<string, DbType> columnTypeMap)
        {
            if (columnDefinitions == null || columnTypeMap == null)
            {
                columnDefinitions = new List<FieldDTO>();
            }

            var supportedColumnTypes = new HashSet<DbType>() { DbType.String, DbType.Int32, DbType.Boolean };

            // Match columns and filter by supported column type.
            List<FieldDTO> matchedColumns;
            if (string.IsNullOrEmpty(selectedObject))
            {
                matchedColumns = new List<FieldDTO>();
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

                        return new FieldDTO() { Key = columnName, Value = columnName };
                    })
                    .ToList();
            }

            return matchedColumns;
        }
    }
}