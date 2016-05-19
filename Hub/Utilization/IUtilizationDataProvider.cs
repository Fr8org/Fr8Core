namespace Hub.Utilization
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

    public interface IUtilizationDataProvider
    {
        void UpdateActivityExecutionRates(ActivityExecutionRate[] reports);
        OverheatingUsersUpdateResults UpdateOverheatingUsers(int threshold);
        string[] GetOverheatingUsers();
    }
}

