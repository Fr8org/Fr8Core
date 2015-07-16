using System.Collections.Generic;
using System.Data;

namespace Core.Plugins.AzureSql
{
    public interface IDbProvider
    {
        /// <summary>
        /// Check if table exists.
        /// </summary>
        bool TableExists(IDbTransaction tx, string schema, string table);

        /// <summary>
        /// Write data row to table.
        /// </summary>
        void WriteRow(IDbTransaction tx, string schema, 
           string table, IEnumerable<FieldValue> values);
    }
}
