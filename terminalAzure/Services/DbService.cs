using System;
using System.Collections.Generic;
using System.Data;
using Fr8.Infrastructure.Utilities.Logging;
using TerminalSqlUtilities;
using terminalAzure.Infrastructure;

namespace terminalAzure.Services
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
            var dbProvider = DbProvider.GetDbProvider(args.ProviderName);
            // Check that terminal knows how to work wih specified provider name.
            if (dbProvider == null)
            {
                var message = string.Format("No DbProvider found for \"{0}\"", args.ProviderName);
                //Logger.GetLogger().Error(message);
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
                    //Logger.GetLogger().Error(ex.Message, ex);
                    Logger.GetLogger().Error($"{ex}");
                    throw;
                }
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
                //Logger.GetLogger().Error(message, ex);
                Logger.GetLogger().Error($"{message}. Exception = {ex}");

                throw new Exception(message);
            }
        }

        /// <summary>
        /// Ensure requested table exists in database.
        /// </summary>
        /// <param name="table">The table we're validating.</param>
        private void EnsureTableExists(IDbProvider dbProvider, IDbTransaction tx, Table table)
        {
            if (!dbProvider.IsTableExisting(tx, table.TableInfo.SchemaName, table.TableInfo.TableName))
            {
                string message;
                if (!string.IsNullOrEmpty(table.TableInfo.SchemaName))
                {
                    message = string.Format(
                        "No table \"{0}.{1}\" found on remote database",
                        table.TableInfo.SchemaName,
                        table.TableInfo.TableName);
                }
                else
                {
                    message = string.Format(
                        "No table \"{0}\" found on remote database",
                        table.TableInfo.TableName);
                }

                //Logger.GetLogger().Error(message);
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
                        dbProvider.WriteRow(tx, table.TableInfo.SchemaName,
                            table.TableInfo.TableName, row.Values);
                    }
                }

                tx.Commit();
            }
        }
    }
}