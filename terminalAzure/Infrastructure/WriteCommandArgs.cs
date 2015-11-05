using System.Collections.Generic;
using TerminalSqlUtilities;

namespace terminalAzure.Infrastructure
{
    public class WriteCommandArgs
    {
        public WriteCommandArgs(string providerName,
            string connectionString, IEnumerable<Table> tables)
        {
            _providerName = providerName;
            _connectionString = connectionString;
            _tables = tables;
        }

        public string ProviderName { get { return _providerName; } }

        public string ConnectionString { get { return _connectionString; } }

        public IEnumerable<Table> Tables { get { return _tables; } }


        private readonly string _providerName;
        private readonly string _connectionString;
        private readonly IEnumerable<Table> _tables;
    }
}
