using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Data.Entities;
using Newtonsoft.Json;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using TerminalSqlUtilities;
using TerminalBase.BaseClasses;
using Utilities.Configuration.Azure;

namespace terminalFr8Core.Infrastructure
{
    public class FindObjectHelper
    {
        private const string DefaultDbProvider = "System.Data.SqlClient";

        public async Task<Dictionary<string, DbType>> ExtractColumnTypes(
            BaseTerminalAction action, ActionDO actionDO)
        {
            var upstreamCrates = await action.GetCratesByDirection<StandardDesignTimeFieldsCM>(
                actionDO,
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

            var columnTypeMap = new Dictionary<string, DbType>();
            foreach (var columnType in columnTypeFields)
            {
                columnTypeMap.Add(columnType.Key, (DbType)Enum.Parse(typeof(DbType), columnType.Value));
            }

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
    }
}