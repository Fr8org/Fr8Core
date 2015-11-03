namespace TerminalSqlUtilities
{
    public class ColumnInfo
    {
        public ColumnInfo(TableInfo tableInfo, string columnName)
        {
            _tableInfo = tableInfo;
            _columnName = columnName;
        }

        public TableInfo TableInfo { get { return _tableInfo; } }

        public string ColumnName { get { return _columnName; } }


        private readonly TableInfo _tableInfo;
        private readonly string _columnName;
    }
}
