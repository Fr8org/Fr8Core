using System.Collections.Generic;

namespace pluginAzureSqlServer.Infrastructure
{
    public class Table
    {
        public Table(string schema, string table, IEnumerable<Row> rows)
        {
            _schema = schema;
            _table = table;
            _rows = rows;
        }

        public string Schema { get { return _schema; } }

        public string TableName { get { return _table; } }

        public IEnumerable<Row> Rows { get { return _rows; } }


        private readonly string _schema;
        private readonly string _table;
        private readonly IEnumerable<Row> _rows;
    }
}
