using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace TerminalSqlUtilities
{
    public class SelectQuery
    {
        public SelectQuery(
            string connectionString,
            TableInfo tableInfo,
            IEnumerable<ColumnInfo> columns,
            IEnumerable<FilterConditionDTO> conditions)
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

        public IEnumerable<FilterConditionDTO> Conditions
        {
            get { return _conditions; }
        }


        private readonly string _connectionString;
        private readonly TableInfo _tableInfo;
        private readonly IEnumerable<ColumnInfo> _columns;
        private readonly IEnumerable<FilterConditionDTO> _conditions;
    }
}
