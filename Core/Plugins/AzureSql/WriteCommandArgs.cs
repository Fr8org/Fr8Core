using System.Collections.Generic;

namespace Core.Plugins.AzureSql
{
    public class WriteCommandArgs
    {
        public WriteCommandArgs(string provider,
            string connectionString, string schema,
            string table, IEnumerable<Row> rows)
        {
            _provider = provider;
            _connectionString = connectionString;
            _schema = schema;
            _table = table;
            _rows = rows;
        }

        public string Provider { get { return _provider; } }

        public string ConnectionString { get { return _connectionString; } }

        public string Schema { get { return _schema; } }

        public string Table { get { return _table; } }

        public IEnumerable<Row> Rows { get { return _rows; } }


        private readonly string _provider;
        private readonly string _connectionString;
        private readonly string _schema;
        private readonly string _table;
        private readonly IEnumerable<Row> _rows;
    }
}
