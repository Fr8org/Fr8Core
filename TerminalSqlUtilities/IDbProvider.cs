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
    }
}
