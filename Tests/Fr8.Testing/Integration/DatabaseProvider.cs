using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace Fr8.Testing.Integration
{
    /// <summary>
    /// An utility class simplifying the execution of SQL scripts for database-related metric retrieval. 
    /// </summary>
    public class DatabaseProvider : IDisposable
    {
        private SqlConnection _conn;
        private SqlCommand _comm;
        private SqlDataReader _reader;

        private DatabaseProvider() { }

        public string Database
        {
            get
            {
                return _conn.Database;
            }
        }
        public static DatabaseProvider Create(string scriptName, string connectionString)
        {
            var provider = new DatabaseProvider();
            provider.Init(scriptName, connectionString);
            return provider;
        }

        private string GetScriptPath(string rootPath, string scriptName)
        {
            return Path.Combine(rootPath, "SQL", scriptName);
        }

        private void Init(string scriptName, string connectionString)
        {
            string commandText;
            string rootPath = Fr8.Infrastructure.Utilities.MiscUtils.UpNLevels(Environment.CurrentDirectory, 2);
            string sqlScript = GetScriptPath(rootPath, scriptName);
            if (File.Exists(sqlScript))
                commandText = File.ReadAllText(sqlScript);
            else
            {
                //Check alternative location (for HM deployed as a Web Job)
                rootPath = Environment.CurrentDirectory;
                sqlScript = GetScriptPath(rootPath, scriptName);
                if (File.Exists(sqlScript))
                {
                    commandText = File.ReadAllText(sqlScript);
                }
                else
                {
                    throw new FileNotFoundException($"The SQL script is not found in this location: {sqlScript}");
                }
            }
            _conn = new SqlConnection(connectionString);
            _conn.Open();

            _comm = new SqlCommand(commandText, _conn);
            _comm.CommandTimeout = 300;
        }

        public SqlDataReader ExecuteReader()
        {
            _reader = _comm.ExecuteReader();
            return _reader;
        }

        public object ExecuteScalar()
        {
            var result = _comm.ExecuteScalar();
            return result;
        }

        public void Dispose()
        {
            if (_reader != null && !_reader.IsClosed)
                _reader.Close();

            if (_conn.State != ConnectionState.Closed)
                _conn.Close();
        }
    }
}
