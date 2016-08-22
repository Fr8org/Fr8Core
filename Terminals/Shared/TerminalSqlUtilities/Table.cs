using System.Collections.Generic;

namespace TerminalSqlUtilities
{
    public class Table
    {
        public Table(TableInfo tableInfo, IEnumerable<Row> rows)
        {
            _tableInfo = tableInfo;
            _rows = rows;
        }

        public TableInfo TableInfo { get { return _tableInfo; } }

        public IEnumerable<Row> Rows { get { return _rows; } }


        private readonly TableInfo _tableInfo;
        private readonly IEnumerable<Row> _rows;
    }
}
