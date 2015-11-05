using System.Collections.Generic;

namespace TerminalSqlUtilities
{
    /// <summary>
    /// Class to represent built Sql-query with command text and params.
    /// </summary>
    public class SqlQuery
    {
        public SqlQuery(string sql, IEnumerable<SqlQueryParameter> parameters)
        {
            _sql = sql;
            _parameters = parameters;
        }

        public string Sql { get { return _sql; } }

        public IEnumerable<SqlQueryParameter> Parameters { get { return _parameters; } }


        private readonly string _sql;
        private readonly IEnumerable<SqlQueryParameter> _parameters;
    }
}
