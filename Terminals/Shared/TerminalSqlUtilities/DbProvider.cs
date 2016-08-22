namespace TerminalSqlUtilities
{
    public class DbProvider
    {
        /// <summary>
        /// Get corresponding IDbProvider against ADO.NET connection provider string.
        /// </summary>
        /// <param name="provider">ADO.NET connection provider string.</param>
        public static IDbProvider GetDbProvider(string provider)
        {
            switch (provider)
            {
                case "System.Data.SqlClient":
                    return new SqlClientDbProvider();

                default:
                    return null;
            }
        }
    }
}
