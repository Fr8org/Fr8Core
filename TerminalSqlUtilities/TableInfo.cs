namespace TerminalSqlUtilities
{
    public class TableInfo
    {
        public TableInfo(string schemaName, string tableName)
        {
            _schemaName = schemaName;
            _tableName = tableName;
        }

        public string SchemaName
        {
            get { return _schemaName; }
        }

        public string TableName
        {
            get { return _tableName; }
        }

        private readonly string _schemaName;
        private readonly string _tableName;
    }
}
