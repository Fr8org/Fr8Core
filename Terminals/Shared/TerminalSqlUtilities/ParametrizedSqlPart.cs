using System.Collections.Generic;

namespace TerminalSqlUtilities
{
    internal class ParametrizedSqlPart
    {
        public string Sql { get; set; }
        public IEnumerable<SqlQueryParameter> Parameters { get; set; }
    }
}
