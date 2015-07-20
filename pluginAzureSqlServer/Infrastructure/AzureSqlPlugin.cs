using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Logging;

namespace pluginAzureSqlServer.Infrastructure
{
    public class AzureSqlPlugin : IAzureSqlPlugin
    {
        public void WriteCommand(WriteCommandArgs args)
        {
            var dbProvider = GetDbProvider(args.Provider);
            if (dbProvider == null)
            {
                var message = string.Format("No DbProvider found for \"{0}\"", args.Provider);
                Logger.GetLogger().Error(message);

                throw new Exception(message);
            }

            using (var dbconn = dbProvider.CreateConnection(args.ConnectionString))
            {
                // Try connect to remote server.
                try
                {
                    dbconn.Open();
                }
                catch (Exception ex)
                {
                    var message = string.Format("Could not connect to remote database \"{0}\"", args.ConnectionString);
                    Logger.GetLogger().Error(message, ex);

                    throw new Exception(message);
                }

                try
                {
                    using (var tx = dbconn.BeginTransaction())
                    {
                        foreach (var table in args.Tables)
                        {
                            // Validate that target table exists on remote database.
                            if (!dbProvider.TableExists(tx, table.Schema, table.TableName))
                            {
                                string message;
                                if (!string.IsNullOrEmpty(table.Schema))
                                {
                                    message = string.Format("No table \"{0}.{1}\" found on remote database", table.Schema, table.TableName);
                                }
                                else
                                {
                                    message = string.Format("No table \"{0}\" found on remote database", table.TableName);
                                }

                                Logger.GetLogger().Error(message);

                                throw new Exception(message);
                            }

                            // Insert rows.
                            foreach (var row in table.Rows)
                            {
                                dbProvider.WriteRow(tx, table.Schema, table.TableName, row.Values);
                            }
                        }

                        tx.Commit();
                    }
                }
                catch (Exception ex)
                {
                    Logger.GetLogger().Error(ex.Message, ex);
                    throw;
                }
            }
        }

        protected IDbProvider GetDbProvider(string provider)
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
