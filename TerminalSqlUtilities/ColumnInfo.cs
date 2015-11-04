using System.Data;

namespace TerminalSqlUtilities
{
    public class ColumnInfo
    {
        public ColumnInfo(TableInfo tableInfo, string columnName, DbType dbType)
        {
            _tableInfo = tableInfo;
            _columnName = columnName;
            _dbType = dbType;
        }

        public TableInfo TableInfo { get { return _tableInfo; } }

        public string ColumnName { get { return _columnName; } }

        public DbType DbType { get { return _dbType; } }

        
        private readonly TableInfo _tableInfo;
        private readonly string _columnName;
        private readonly DbType _dbType;
    }
}
