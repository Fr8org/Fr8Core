namespace Hub.Utilization.Impl
{
    public class MockedUtilizationDataProvider : IUtilizationDataProvider
    {
        public virtual void UpdateActivityExecutionRates(ActivityExecutionRate[] reports)
        {
        }

        public virtual OverheatingUsersUpdateResults UpdateOverheatingUsers(int threshold)
        {
            return new OverheatingUsersUpdateResults(new string[0],  new string[0]);
        }

        public virtual string[] GetOverheatingUsers()
        {
            return new string[0];
        }
    }
}
