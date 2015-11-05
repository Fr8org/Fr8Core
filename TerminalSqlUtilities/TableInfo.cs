using System;

namespace TerminalSqlUtilities
{
    public class TableInfo
    {
        public TableInfo(string fullTableName)
        {
            var tableNameTokens = fullTableName.Split(
                new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            if (tableNameTokens.Length == 1)
            {
                _schemaName = "";
                _tableName = tableNameTokens[0];
            }
            else if (tableNameTokens.Length == 2)
            {
                _schemaName = tableNameTokens[0];
                _tableName = tableNameTokens[1];
            }
            else
            {
                throw new ApplicationException("Invalid table name.");
            }
        }

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

        public override string ToString()
        {
            if (string.IsNullOrEmpty(_schemaName))
            {
                return _tableName;
            }
            else
            {
                return string.Join(".", _schemaName, _tableName);
            }
        }


        private readonly string _schemaName;
        private readonly string _tableName;
    }
}
