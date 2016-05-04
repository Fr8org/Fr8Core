using System;
using System.Collections.Generic;
using System.Threading;
using Data.Entities;
using Utilities.Configuration.Azure;

namespace Hub.Utilization
{
    public class UtilizationMonitoringService : IDisposable
    {
        private class UtilizationReport
        {
            public int ActivitiesExecuted
            {
                get;
                set;
            }
        }

        private const int DefaultReportAggregationUnit = 120; // in seconds

        private readonly Dictionary<string, UtilizationReport> _utilizationReports = new Dictionary<string, UtilizationReport>();
        private readonly Timer _reportTimer;
        private readonly int _reportAggregationUnit;

        public UtilizationMonitoringService()
        {
            var aggregationUnitStr = CloudConfigurationManager.GetSetting("UtilizationReportAggregationUnit");
            
            if (string.IsNullOrWhiteSpace(aggregationUnitStr) || !int.TryParse(aggregationUnitStr, out _reportAggregationUnit))
            {
                _reportAggregationUnit = DefaultReportAggregationUnit;
            }

            _reportTimer = new Timer(OnReportTimerTick, this, _reportAggregationUnit * 1000, _reportAggregationUnit * 1000);
        }

        // this method is static to allow GC of UtilizationMonitoringService instance.
        private static void OnReportTimerTick(object state)
        {
            var that = (UtilizationMonitoringService) state;
            that.AggregateUtilizationReport();
        }

        private void AggregateUtilizationReport()
        {
            lock (_utilizationReports)
            {
                foreach (var utilizationReport in _utilizationReports)
                {
                    
                }
            }
        }

        private string ExtractUser(ActivityDO activity, ContainerDO container)
        {
            return activity.Fr8AccountId;
        }

        public void TrackActivityExecution(ActivityDO activity, ContainerDO container)
        {
            lock (_utilizationReports)
            {
                UtilizationReport report;

                var user = ExtractUser(activity, container);

                if (!_utilizationReports.TryGetValue(user, out report))
                {
                    report = new UtilizationReport();
                    _utilizationReports.Add(user, report);
                }

                report.ActivitiesExecuted ++;
            }
        }

        public void Dispose()
        {
            _reportTimer.Dispose();
        }
    }
}
