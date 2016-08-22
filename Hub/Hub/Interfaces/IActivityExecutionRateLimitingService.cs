namespace Hub.Interfaces
{
    public interface IActivityExecutionRateLimitingService
    {
        bool CheckActivityExecutionRate(string fr8AccountId);
    }
}