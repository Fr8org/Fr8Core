namespace TerminalSqlUtilities
{
    public class ColumnInfo
    {
        public ColumnInfo(TableInfo table, string columnName)
        {
            _table = table;
            _columnName = columnName;
        }

        public TableInfo Table { get { return _table; } }

        public string ColumnName { get { return _columnName; } }


        private readonly TableInfo _table;
        private readonly string _columnName;
    }
}
