using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace TerminalSqlUtilities
{
    public class SqlClientDbProvider : IDbProvider
    {
        private readonly DbHelper _dbHelper = new DbHelper();
        private readonly SqlClientSqlQueryBuilder _queryBuilder = new SqlClientSqlQueryBuilder();


        public IDbConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public bool IsTableExisting(IDbTransaction tx, string schema, string table)
        {
            using (var cmd = tx.Connection.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"SELECT COUNT(*) FROM [INFORMATION_SCHEMA].[TABLES]
                    WHERE [TABLE_SCHEMA] = @schema AND [TABLE_NAME] = @table";

                var schemaParam = cmd.CreateParameter();
                schemaParam.ParameterName = "@schema";
                schemaParam.Value = !string.IsNullOrEmpty(schema) ? schema : "dbo";
                cmd.Parameters.Add(schemaParam);

                var tableParam = cmd.CreateParameter();
                tableParam.ParameterName = "@table";
                tableParam.Value = table;
                cmd.Parameters.Add(tableParam);

                var count = Convert.ToInt64(cmd.ExecuteScalar());
                return (count > 0);
            }
        }

        public void WriteRow(IDbTransaction tx, string schema,
            string table, IEnumerable<FieldValue> values)
        {
            if (!string.IsNullOrEmpty(schema))
            {
                EnsureValidIdentifier(schema);
            }

            EnsureValidIdentifier(table);

            using (var cmd = tx.Connection.CreateCommand())
            {
                var fieldsListBuilder = new StringBuilder();
                var paramsListBuilder = new StringBuilder();
                var paramCount = 0;
                foreach (var value in values)
                {
                    EnsureValidIdentifier(value.Field);

                    if (paramCount > 0)
                    {
                        fieldsListBuilder.Append(", ");
                        paramsListBuilder.Append(", ");
                    }

                    // Adding to fields list.
                    fieldsListBuilder.Append("[");
                    fieldsListBuilder.Append(value.Field);
                    fieldsListBuilder.Append("]");

                    // Adding to params list.
                    paramsListBuilder.Append("@param_");
                    paramsListBuilder.Append(paramCount);

                    ++paramCount;
                }                

                cmd.Transaction = tx;
                cmd.CommandText = string.Format(
                    "INSERT INTO [{0}].[{1}] ({2}) VALUES ({3})",
                    !string.IsNullOrEmpty(schema) ? schema : "dbo",
                    table, fieldsListBuilder.ToString(),
                    paramsListBuilder.ToString()
                    );

                var i = 0;
                foreach (var value in values)
                {
                    var valueParam = cmd.CreateParameter();
                    valueParam.ParameterName = "@param_" + i;
                    valueParam.Value = value.Value.ToString();
                    cmd.Parameters.Add(valueParam);

                    ++i;
                }

                cmd.ExecuteNonQuery();
            }            
        }

        /// <summary>
        /// Get full column name in database syntax.
        /// </summary>
        public string GetFullColumnName(ColumnInfo columnInfo)
        {
            if (string.IsNullOrEmpty(columnInfo.TableInfo.SchemaName))
            {
                return string.Format(
                    "[{0}].[{1}]",
                    columnInfo.TableInfo.TableName,
                    columnInfo.ColumnName
                );
            }
            else
            {
                return string.Format(
                    "[{0}].[{1}].[{2}]",
                    columnInfo.TableInfo.SchemaName,
                    columnInfo.TableInfo.TableName,
                    columnInfo.ColumnName
                );
            }
        }

        /// <summary>
        /// List all columns from database.
        /// </summary>
        public IEnumerable<ColumnInfo> ListAllColumns(IDbTransaction tx)
        {
            using (var cmd = tx.Connection.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT
	                    [c].[TABLE_SCHEMA],
	                    [c].[TABLE_NAME],
	                    [c].[COLUMN_NAME],
                        [c].[DATA_TYPE]
                    FROM [INFORMATION_SCHEMA].[COLUMNS] [c]";

                cmd.Transaction = tx;

                using (var reader = cmd.ExecuteReader())
                {
                    var columns = new List<ColumnInfo>();

                    while (reader.Read())
                    {
                        var schemaName = reader.GetString(0);
                        var tableName = reader.GetString(1);
                        var columnName = reader.GetString(2);
                        var dbType = MapDbType(reader.GetString(3));

                        columns.Add(
                            new ColumnInfo(
                                new TableInfo(schemaName, tableName),
                                columnName,
                                dbType
                            )
                        );
                    }

                    return columns;
                }
            }
        }

        /// <summary>
        /// Map string data-type name to System.Data.DbType.
        /// </summary>
        private DbType MapDbType(string dataType)
        {
            var dataTypeUpper = dataType.ToUpper();

            if (dataTypeUpper.Contains("VARCHAR"))
            {
                return DbType.String;
            }
            else if (dataTypeUpper == "INT")
            {
                return DbType.Int32;
            }
            else if (dataTypeUpper.Contains("DATE") || dataTypeUpper.Contains("TIME"))
            {
                return DbType.DateTime;
            }
            else if (dataTypeUpper == "BIT")
            {
                return DbType.Boolean;
            }
            else if (dataTypeUpper.Contains("BINARY"))
            {
                return DbType.Binary;
            }
            else if (dataTypeUpper == "UNIQUEIDENTIFIER")
            {
                return DbType.Guid;
            }
            else
            {
                throw new NotSupportedException("Unknown data type");
            }
        }

        /// <summary>
        /// Ensure that string is a valid SQL-identifier.
        /// </summary>
        private void EnsureValidIdentifier(string id)
        {
            var valid = !string.IsNullOrEmpty(id) && !id.Contains("[") && !id.Contains("]");
            if (!valid)
            {
                throw new Exception(string.Format("Invalid identifier \"{0}\"", id));
            }
        }

        /// <summary>
        /// Execute query against database and return table data.
        /// </summary>
        public Table ExecuteQuery(SelectQuery query)
        {
            var sqlQuery = _queryBuilder.BuildSelectQuery(query);

            using (var conn = CreateConnection(query.ConnectionString))
            {
                conn.Open();

                using (var tx = conn.BeginTransaction())
                {
                    using (var cmd = tx.Connection.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = sqlQuery.Sql;

                        foreach (var parameter in sqlQuery.Parameters)
                        {
                            var dbParam = cmd.CreateParameter();
                            dbParam.ParameterName = parameter.Name;
                            dbParam.Value = parameter.Value;

                            cmd.Parameters.Add(dbParam);
                        }

                        using (var reader = cmd.ExecuteReader())
                        {
                            var dataTable = _dbHelper.ExtractDataTable(query.TableInfo, reader);
                            return dataTable;
                        }
                    }
                }
            }
        }
    }
}
