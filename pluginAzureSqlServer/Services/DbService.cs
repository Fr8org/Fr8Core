using System;
using System.Collections.Generic;
using System.Data;
using terminal_AzureSqlServer.Infrastructure;
using Utilities.Logging;

namespace terminal_AzureSqlServer.Services
{
    public class DbService : IDbService
    {
        /// <summary>
        /// Write some data to remote database.
        /// </summary>
        /// <param name="args">Connection string, provider name, tables data.</param>
        public void WriteCommand(WriteCommandArgs args)
        {
            // Get corresponding provider.
            var dbProvider = GetDbProvider(args.ProviderName);
            // Check that terminal knows how to work wih specified provider name.
            if (dbProvider == null)
            {
                var message = string.Format("No DbProvider found for \"{0}\"", args.ProviderName);
                Logger.GetLogger().Error(message);

                throw new Exception(message);
            }

            // Create connection and insert data to database.
            using (var dbconn = dbProvider.CreateConnection(args.ConnectionString))
            {
                OpenConnection(dbconn);

                try
                {
                    WriteDataToDb(dbProvider, dbconn, args.Tables);
                }
                catch (Exception ex)
                {
                    Logger.GetLogger().Error(ex.Message, ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// Get corresponding IDbProvider against ADO.NET connection provider string.
        /// </summary>
        /// <param name="provider">ADO.NET connection provider string.</param>
        private IDbProvider GetDbProvider(string provider)
        {
            switch (provider)
            {
                case "System.Data.SqlClient":
                    return new SqlClientDbProvider();

                default:
                    return null;
            }
        }

        /// <summary>
        /// Try open database connection.
        /// </summary>
        private void OpenConnection(IDbConnection dbconn)
        {
            try
            {
                dbconn.Open();
            }
            catch (Exception ex)
            {
                var message = string.Format("Could not connect to remote database \"{0}\"", dbconn.ConnectionString);
                Logger.GetLogger().Error(message, ex);

                throw new Exception(message);
            }
        }

        /// <summary>
        /// Ensure requested table exists in database.
        /// </summary>
        /// <param name="table">The table we're validating.</param>
        private void EnsureTableExists(IDbProvider dbProvider, IDbTransaction tx, Table table)
        {
            if (!dbProvider.IsTableExisting(tx, table.SchemaName, table.TableName))
            {
                string message;
                if (!string.IsNullOrEmpty(table.SchemaName))
                {
                    message = string.Format("No table \"{0}.{1}\" found on remote database", table.SchemaName,
                        table.TableName);
                }
                else
                {
                    message = string.Format("No table \"{0}\" found on remote database", table.TableName);
                }

                Logger.GetLogger().Error(message);

                throw new Exception(message);
            }
        }

        /// <summary>
        /// Write data to database
        /// </summary>
        /// <param name="tables">Data to write.</param>
        private void WriteDataToDb(IDbProvider dbProvider, IDbConnection dbconn, IEnumerable<Table> tables)
        {
            // Process all data insertion as single transaction.
            using (var tx = dbconn.BeginTransaction())
            {
                foreach (var table in tables)
                {
                    // Ensure that requested table exists.
                    EnsureTableExists(dbProvider, tx, table);

                    // Insert rows.
                    foreach (var row in table.Rows)
                    {
                        dbProvider.WriteRow(tx, table.SchemaName, table.TableName, row.Values);
                    }
                }

                tx.Commit();
            }
        }
    }
}