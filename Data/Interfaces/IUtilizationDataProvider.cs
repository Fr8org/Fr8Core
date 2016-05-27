using System;

namespace Data.Interfaces
{
    public class OverheatingUsersUpdateResults
    {
        public string[] StartedOverheating { get; private set; }
        public string[] StoppedOverheating { get; private set; }

        public OverheatingUsersUpdateResults(string[] startedOverheating, string[] stoppedOverheating)
        {
            StartedOverheating = startedOverheating;
            StoppedOverheating = stoppedOverheating;
        }
    }

    public class ActivityExecutionRate
    {
        public string UserId
        {
            get;
            set;
        }

        public int ActivitiesExecuted
        {
            get;
            set;
        }
    }

    public interface IUtilizationDataProvider
    {
        void UpdateActivityExecutionRates(ActivityExecutionRate[] reports);
        OverheatingUsersUpdateResults UpdateOverheatingUsers(int threshold, TimeSpan metricReportValidTime, TimeSpan banTime);
        string[] GetOverheatingUsers();
    }
}

