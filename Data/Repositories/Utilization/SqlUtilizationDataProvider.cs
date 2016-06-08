using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Data.Interfaces;
using Data.Repositories.SqlBased;
using log4net;

namespace Data.Repositories.Utilization
{
    public class SqlUtilizationDataProvider : IUtilizationDataProvider
    {
        private readonly ISqlConnectionProvider _connectionProvider;
        private static readonly ILog Logger = Fr8.Infrastructure.Utilities.Logging.Logger.GetCurrentClassLogger();

        public SqlUtilizationDataProvider(ISqlConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public void UpdateActivityExecutionRates(ActivityExecutionRate[] reports)
        {
            if (reports == null || reports.Length == 0)
            {
                return;
            }

            try
            {
                using (var connection = OpenConnection())
                using (var command = new SqlCommand {Connection = connection})
                {
                    var commandTextBuilder = new StringBuilder();

                    commandTextBuilder.AppendLine("MERGE INTO dbo.UtilizationMetrics AS Target USING (VALUES");

                    command.Parameters.AddWithValue("@LastUpdated", DateTimeOffset.Now);

                    commandTextBuilder.AppendLine("(@userId0, @epu0)");
                    command.Parameters.AddWithValue("@userId0", reports[0].UserId);
                    command.Parameters.AddWithValue("@epu0", reports[0].ActivitiesExecuted);

                    for (int i = 1; i < reports.Length; i ++)
                    {
                        commandTextBuilder.AppendLine($", (@userId{i}, @epu{i})");
                        command.Parameters.AddWithValue("@userId" + i, reports[i].UserId);
                        command.Parameters.AddWithValue("@epu" + i, reports[i].ActivitiesExecuted);
                    }

                    commandTextBuilder.AppendLine(@")
                       AS Source(UserId, EPU)
                       ON Target.UserId = Source.UserId
                       WHEN MATCHED THEN
                       UPDATE SET LastUpdated = @LastUpdated, EPU = Source.EPU
                       WHEN NOT MATCHED BY TARGET THEN
                       INSERT(UserId, EPU, LastUpdated) VALUES(UserId, EPU, @LastUpdated);");

                    command.CommandText = commandTextBuilder.ToString();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to update activities execution rates",ex);
            }
        }

        public OverheatingUsersUpdateResults UpdateOverheatingUsers(int threshold, TimeSpan metricReportValidTime, TimeSpan banTime)
        {
            try
            {
                using (var connection = OpenConnection())
                using (var command = new SqlCommand {Connection = connection})
                {

                    command.CommandText = @"MERGE INTO dbo.RateLimiterState as target
                                            USING dbo.UtilizationMetrics AS source
                                            ON (target.UserId = source.UserId )

                                        WHEN MATCHED AND EPU > @EpuThreshold and LastUpdated > @ValidTime and target.IsOverheating = 0 THEN 
                                            UPDATE SET BlockTill = @BlockTill, IsOverheating = 1

                                        WHEN NOT MATCHED AND EPU > @EpuThreshold  and LastUpdated > @ValidTime THEN
                                            INSERT (UserId, IsOverheating, BlockTill) VALUES (source.UserId, 1, @BlockTill)
                                        OUTPUT  inserted.UserId;";

                    command.Parameters.AddWithValue("@EpuThreshold", threshold);
                    command.Parameters.AddWithValue("@BlockTill", DateTimeOffset.Now + banTime);
                    command.Parameters.AddWithValue("@ValidTime", DateTimeOffset.Now - metricReportValidTime);

                    var overheatingUsers = new List<string>();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            overheatingUsers.Add((string)reader["UserId"]);
                        }
                    }

                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@Now", DateTimeOffset.Now);
                    command.CommandText = @"update dbo.RateLimiterState set IsOverheating = 0, BlockTill = NULL 
                                           output inserted.UserId 
                                           where BlockTill < @Now ";

                    var resumedUsers = new List<string>();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resumedUsers.Add((string)reader["UserId"]);
                        }
                    }

                    return new OverheatingUsersUpdateResults(overheatingUsers.ToArray(), resumedUsers.ToArray());
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to update overheating user states", ex);
                throw;
            }
        }

        public string[] GetOverheatingUsers()
        {
            try
            {
                using (var connection = OpenConnection())
                using (var command = new SqlCommand {Connection = connection})
                {
                    command.Parameters.Clear();
                    command.CommandText = "select UserId from dbo.RateLimiterState where IsOverheating = 1";

                    var overheatingUsers = new List<string>();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            overheatingUsers.Add((string)reader["UserId"]);
                        }
                    }

                    return overheatingUsers.ToArray();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to get overheating users", ex);
                throw;
            }
        }


        private SqlConnection OpenConnection()
        {
            var connection = new SqlConnection((string)_connectionProvider.ConnectionInfo);

            connection.Open();
            
            return connection;
        }
    }
}
