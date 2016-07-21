using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Hub.Interfaces;

namespace Hub.Services
{
    public class UtilizationMonitoringService : IDisposable, IUtilizationMonitoringService
    {
        private const int DefaultReportAggregationUnit = 120; // in seconds
        private const int MinimalReportAggregationUnitDuration = 1;

        private readonly IUtilizationDataProvider _utilizationDataProvider;
        private readonly Dictionary<string, ActivityExecutionRate> _activityExecutionReports = new Dictionary<string, ActivityExecutionRate>();
        private readonly ITimer _reportTimer;
        private readonly int _reportAggregationUnit;

        public int AggregationUnitDuration => _reportAggregationUnit;

        public UtilizationMonitoringService(IUtilizationDataProvider utilizationDataProvider, ITimer timer)
        {
            var aggregationUnitStr = CloudConfigurationManager.GetSetting("UtilizationReportAggregationUnit");
            
            if (string.IsNullOrWhiteSpace(aggregationUnitStr) || !int.TryParse(aggregationUnitStr, out _reportAggregationUnit))
            {
                _reportAggregationUnit = DefaultReportAggregationUnit;
            }

            _reportAggregationUnit = Math.Max(MinimalReportAggregationUnitDuration, _reportAggregationUnit);

            _utilizationDataProvider = utilizationDataProvider;

            _reportTimer = timer;
            timer.Configure(OnReportTimerTick, this, _reportAggregationUnit * 1000, _reportAggregationUnit * 1000);
        }

        // this method is static to allow GC of UtilizationMonitoringService instance.
        private static void OnReportTimerTick(object state)
        {
            var that = (UtilizationMonitoringService) state;
            that.UpdateActivityExecutionRates();
        }

        private void UpdateActivityExecutionRates()
        {
            lock (_activityExecutionReports)
            {
                _utilizationDataProvider.UpdateActivityExecutionRates(_activityExecutionReports.Values.ToArray());
                _activityExecutionReports.Clear();
            }
        }

        private string ExtractUser(ActivityDO activity, ContainerDO container)
        {
            return activity.Fr8AccountId;
        }

        public void TrackActivityExecution(ActivityDO activity, ContainerDO container)
        {
            lock (_activityExecutionReports)
            {
                ActivityExecutionRate report;

                var user = ExtractUser(activity, container);

                if (!_activityExecutionReports.TryGetValue(user, out report))
                {
                    report = new ActivityExecutionRate
                    {
                        UserId = user
                    };
                    
                    _activityExecutionReports.Add(user, report);
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
