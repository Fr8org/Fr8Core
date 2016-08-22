using System.Collections.Generic;
using System.Data;

namespace TerminalSqlUtilities
{
    public interface IDbProvider
    {
        /// <summary>
        /// Create ADO.NET connection to remote db.
        /// </summary>
        IDbConnection CreateConnection(string connectionString);

        /// <summary>
        /// Check if table exists.
        /// </summary>
        bool IsTableExisting(IDbTransaction tx, string schema, string table);

        /// <summary>
        /// List all columns from database.
        /// </summary>
        IEnumerable<ColumnInfo> ListAllColumns(IDbTransaction tx);

        /// <summary>
        /// Get full column name in database syntax.
        /// </summary>
        string GetFullColumnName(ColumnInfo columnInfo);

        /// <summary>
        /// Write data row to table.
        /// </summary>
        void WriteRow(IDbTransaction tx, string schema, 
           string table, IEnumerable<FieldValue> values);

        /// <summary>
        /// Execute query against database, and return table result.
        /// </summary>
        Table ExecuteQuery(SelectQuery query);

        /// <summary>
        /// Gets List of All Tables in DB
        /// </summary>
        IEnumerable<TableInfo> ListAllTables(IDbTransaction tx);
        
        /// <summary>
        /// Gets Identity Column of Table
        /// </summary>
        string GetIdentityColumn(IDbTransaction tx, string tableName);

        /// <summary>
        /// Gets All Columns in Table
        /// </summary>
        IEnumerable<ColumnInfo> ListTableColumns(IDbTransaction tx, string tablename);
    }
}
