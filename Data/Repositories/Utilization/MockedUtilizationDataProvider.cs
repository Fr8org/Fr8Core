using System;
using Data.Interfaces;

namespace Data.Repositories.Utilization
{
    public class MockedUtilizationDataProvider : IUtilizationDataProvider
    {
        public virtual void UpdateActivityExecutionRates(ActivityExecutionRate[] reports)
        {
        }

        public virtual OverheatingUsersUpdateResults UpdateOverheatingUsers(int threshold, TimeSpan metricReportValidTime, TimeSpan banTime)
        {
            return new OverheatingUsersUpdateResults(new string[0],  new string[0]);
        }

        public virtual string[] GetOverheatingUsers()
        {
            return new string[0];
        }
    }
}
