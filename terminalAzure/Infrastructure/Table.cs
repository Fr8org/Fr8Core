using System.Collections.Generic;

namespace terminalAzure.Infrastructure
{
    public class Table
    {
        public Table(string schemaName, string tableName, IEnumerable<Row> rows)
        {
            _schemaName = schemaName;
            _tableName = tableName;
            _rows = rows;
        }

        public string SchemaName { get { return _schemaName; } }

        public string TableName { get { return _tableName; } }

        public IEnumerable<Row> Rows { get { return _rows; } }


        private readonly string _schemaName;
        private readonly string _tableName;
        private readonly IEnumerable<Row> _rows;
    }
}
