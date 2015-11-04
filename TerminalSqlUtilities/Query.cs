using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;

namespace TerminalSqlUtilities
{
    public class Query
    {
        public Query(
            string connectionString,
            TableInfo tableInfo,
            IEnumerable<ColumnInfo> columns,
            IEnumerable<CriteriaDTO> conditions)
        {
            _connectionString = connectionString;
            _tableInfo = tableInfo;
            _columns = columns;
            _conditions = conditions;
        }

        public string ConnectionString
        {
            get { return _connectionString; }
        }

        public TableInfo TableInfo
        {
            get { return _tableInfo; }
        }

        public IEnumerable<ColumnInfo> Columns
        {
            get { return _columns; }
        }

        public IEnumerable<CriteriaDTO> Conditions
        {
            get { return _conditions; }
        }


        private readonly string _connectionString;
        private readonly TableInfo _tableInfo;
        private readonly IEnumerable<ColumnInfo> _columns;
        private readonly IEnumerable<CriteriaDTO> _conditions;
    }
}
