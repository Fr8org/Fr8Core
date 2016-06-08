using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Fr8.TerminalBase.Errors;
using StructureMap;
using TerminalSqlUtilities;

namespace terminalAzure.Infrastructure
{
    public static class DbProviderExtensions {
        public static object ConnectToSql(this IDbProvider provider, string connectionString, Func<IDbCommand, object> innerAction) {
            if (string.IsNullOrEmpty(connectionString)) {
                //Error point 1
                throw new TerminalCodedException(TerminalErrorCode.SQL_SERVER_CONNECTION_STRING_MISSING);
            }

            //We have a conn string, initiate db connection and open
            var connection = provider.CreateConnection(connectionString);

            try {
                //Open the connection, remember to close!!
                connection.Open();

                //Create a command from the const query to comb all tables/cols in one query
                var command = connection.CreateCommand();

                return innerAction(command);                
            }
            catch (Exception ex) {
                //Should any exception be caught during the process, a connection failed error code is returned with the details
                throw new TerminalCodedException(TerminalErrorCode.SQL_SERVER_CONNECTION_FAILED, ex.Message);
            }
            finally {
                //Ensure the connection is closed after use if still open.
                if (connection.State != System.Data.ConnectionState.Closed) {
                    connection.Close();
                }
            }
        }
    }
}