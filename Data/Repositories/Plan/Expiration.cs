using System;

namespace Data.Repositories.Plan
{
    public interface IExpirationToken
    {
        bool IsExpired();
    }

    public interface IPlanCacheExpirationStrategy
    {
        void SetExpirationCallback(Action callback);
        IExpirationToken NewExpirationToken();
    }
    
}
