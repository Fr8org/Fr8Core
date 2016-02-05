using System;
using System.Data.SqlClient;
using Utilities.Configuration.Azure;

namespace Data.Repositories.Authorization
{
    public class AuthRepositoryLogger
    {
        private readonly string _connectionSstring;

        public AuthRepositoryLogger()
        {
            _connectionSstring = CloudConfigurationManager.GetSetting("AuthRepositoryLoggerConnectionString");
        }

        private SqlConnection OpenConnection()
        {
            if (string.IsNullOrWhiteSpace(_connectionSstring))
            {
                return null;
            }

            var connection = new SqlConnection(_connectionSstring);
            connection.Open();
            return connection;
        }


        public void WriteSuccess(string tokenId, string secret, string opType)
        {
            WriteEntry(tokenId, secret, "success " + opType); 
        }


        public void WriteFailure(string tokenId, string message, string opType)
        {
            WriteEntry(tokenId, message, "failure " + opType);
        }

        private void WriteEntry(string tokenId, string data, string type)
        {
            try
            {
                var connection = OpenConnection();
                if (connection == null)
                {
                    return;
                }

                using (connection)
                {
                    using (var command = new SqlCommand("insert into [DebugDb].[dbo].[AuthRepositoryLog] values (@id, @data, @type)"))
                    {
                        command.Connection = connection;
                        command.Parameters.AddWithValue("id", tokenId);
                        command.Parameters.AddWithValue("data", data);
                        command.Parameters.AddWithValue("type", type);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // logger should not fail. Eat exception
            }
        }

    }
}
