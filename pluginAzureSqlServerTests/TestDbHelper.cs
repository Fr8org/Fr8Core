using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace pluginAzureSqlServerTests
{
    public class TestDbHelper
    {
        public const string ConnectionStringName = "TestDB";

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(GetConnectionString());
        }

        public string GetConnectionString()
        {
            var connectionString = ConfigurationManager
                .ConnectionStrings[ConnectionStringName]
                .ConnectionString;

            return connectionString;
        }

        public void CreateCustomersTable(IDbTransaction tx)
        {
            using (var cmd = tx.Connection.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText =
                    @"CREATE TABLE [dbo].[Customers] (
                        [FirstName] NVARCHAR(100) NOT NULL,
                        [LastName] NVARCHAR(100) NOT NULL,
                        PRIMARY KEY CLUSTERED ([FirstName] ASC, [LastName] ASC)
                    )";

                cmd.ExecuteNonQuery();
            }
        }

        public void DropCustomersTable(IDbTransaction tx)
        {
            using (var cmd = tx.Connection.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"DROP TABLE [dbo].[Customers]";

                cmd.ExecuteNonQuery();
            }
        }

        public bool CustomersTableExists(IDbTransaction tx)
        {
            using (var cmd = tx.Connection.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"SELECT COUNT(*) FROM [INFORMATION_SCHEMA].[TABLES]
                    WHERE [TABLE_SCHEMA] = @schema AND [TABLE_NAME] = @table";

                var schemaParam = cmd.CreateParameter();
                schemaParam.ParameterName = "@schema";
                schemaParam.Value = "dbo";
                cmd.Parameters.Add(schemaParam);

                var tableParam = cmd.CreateParameter();
                tableParam.ParameterName = "@table";
                tableParam.Value = "Customers";
                cmd.Parameters.Add(tableParam);

                var count = Convert.ToInt64(cmd.ExecuteScalar());
                return (count > 0);
            }
        }
    }
}
