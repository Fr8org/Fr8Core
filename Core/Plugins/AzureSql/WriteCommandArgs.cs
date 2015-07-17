using System.Collections.Generic;

namespace Core.Plugins.AzureSql
{
    public class WriteCommandArgs
    {
        public WriteCommandArgs(string provider,
            string connectionString, IEnumerable<Table> tables)
        {
            _provider = provider;
            _connectionString = connectionString;
            _tables = tables;
        }

        public string Provider { get { return _provider; } }

        public string ConnectionString { get { return _connectionString; } }

        public IEnumerable<Table> Tables { get { return _tables; } }


        private readonly string _provider;
        private readonly string _connectionString;
        private readonly IEnumerable<Table> _tables;
    }
}
