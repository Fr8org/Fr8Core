using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.ApplicationInsights;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Collections.Generic;
using Fr8.Testing.Integration;

namespace HealthMonitor.Jobs
{
    /// <summary>
    /// Although looking as a test, this is not a test. This is a job which is maintaned and run by HealthMonitor
    /// but not actually testing anything.  
    /// </summary>
    [Explicit]
    public class MetricMonitor : BaseIntegrationTest
    {
        TelemetryClient _telemetry;

        public override string TerminalName
        {
            get
            {
                return "Telemetry";
            }
        }

        public MetricMonitor()
        {
            var appInsightsInstrumentationKey = Program.Context.InstrumentationKey;
            if (!string.IsNullOrEmpty(appInsightsInstrumentationKey))
            {
                Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.Active.InstrumentationKey = appInsightsInstrumentationKey;
                _telemetry = new TelemetryClient();
            }
        }

        [Test]
        public void Calculate_Table_Metrics()
        {
            if (_telemetry == null)
                return;

            string scope = "Database";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            using (DatabaseProvider provider = DatabaseProvider.Create("CountStats.sql", Program.Context.ConnectionString))
            {
                var reader = provider.ExecuteReader();
                parameters.Add("Database", provider.Database);
                while (reader.Read())
                {
                    for (int col = 1; col < reader.FieldCount; col++) //the first field is table name
                    {
                        parameters["Table"] = reader.GetString(0);
                        parameters["Metric"] = reader.GetName(col);
                        TrackMetric(scope, reader.GetString(0), reader.GetName(col), reader.GetInt64(col), parameters);
                    }
                }
            }
        }

        [Test]
        public void Count_Active_Test_Plans()
        {
            if (_telemetry == null)
                return;

            string scope = "Database";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            using (DatabaseProvider provider = DatabaseProvider.Create("CountActiveTestPlans.sql", Program.Context.ConnectionString))
            {
                int activeTestPlans = (int)provider.ExecuteScalar();
                parameters.Add("Database", provider.Database);
                TrackMetric(scope, FormatMetricName(scope, "Active_Test_Plans"), activeTestPlans, parameters);
            }
        }

        [Test]
        public void Get_Dev_Database_File_Stats()
        {
            if (Program.Context.AllArguments.ContainsKey("devConnectionString"))
            {
                var devCS = (string)Program.Context.AllArguments["devConnectionString"];
                if (devCS != null)
                {
                    GetStatsInDatabase(devCS);
                }
            }
        }

        private void GetStatsInDatabase(string connectionString)
        {
            if (_telemetry == null)
                return;

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string scope = "Database";

            using (DatabaseProvider provider = DatabaseProvider.Create("CountFileStats.sql", connectionString))
            {
                var reader = provider.ExecuteReader();
                parameters.Add("Database", provider.Database);
                while (reader.Read())
                {
                    for (int col = 1; col < reader.FieldCount; col++) //the first field is table name
                    {
                        parameters["Database"] = reader.GetString(0);
                        parameters["Metric"] = reader.GetName(col);
                        TrackMetric(scope, reader.GetString(0), reader.GetName(col), reader.GetInt32(col), parameters);
                    }
                }
            }
        }

        private void TrackMetric(string scope, string subScope, string metricName, long metricValue, Dictionary<string, string> parameters)
        {
            _telemetry.TrackMetric(FormatMetricName(scope, subScope, metricName), metricValue, parameters);
        }

        private void TrackMetric(string scope, string metricName, long metricValue, Dictionary<string, string> parameters)
        {
            _telemetry.TrackMetric(FormatMetricName(scope, metricName), metricValue, parameters);
        }

        private string FormatMetricName(string scope, string table, string indicator)
        {
            return $"{scope}.{table}.{indicator}";
        }

        private string FormatMetricName(string scope, string indicator)
        {
            return $"{scope}.{indicator}";
        }
    }
}
