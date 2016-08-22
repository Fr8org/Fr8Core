using System;

namespace Data.Repositories.Cache
{
    public interface ICacheExpirationStrategy
    {
        void SetExpirationCallback(Action callback);
        IExpirationToken NewExpirationToken();
    }
}
