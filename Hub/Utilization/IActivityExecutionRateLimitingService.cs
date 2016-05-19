namespace Hub.Utilization
{
    public interface IActivityExecutionRateLimitingService
    {
        bool CheckActivityExecutionRate(string fr8AccountId);
    }
}