using System;
using System.Threading;
using Data.Repositories.Plan;
using Data.Repositories.Security.StorageImpl.Cache;

namespace Data.Repositories.Cache
{
    public class SlidingExpirationStrategy : ISecurityCacheExpirationStrategy, IPlanCacheExpirationStrategy, IDisposable
    {
        private class ExpirationToken : IExpirationToken
        {
            private readonly DateTime _expirationTime;

            public ExpirationToken(DateTime expirationTime)
            {
                _expirationTime = expirationTime;
            }

            public bool IsExpired()
            {
                return DateTime.UtcNow > _expirationTime;
            }
        }

        private readonly Timer _timer;
        private readonly TimeSpan _slidingExpiration;
        private Action _expirationCallback;
        private const long MinimalRefreshInterval = 1000;

        public SlidingExpirationStrategy(TimeSpan slidingExpiration)
        {
            var refreshInterval = (long)(Math.Max(_slidingExpiration.TotalMilliseconds / 3, MinimalRefreshInterval));
            _timer = new Timer(InvokeExpirationCallback, null, refreshInterval, refreshInterval);
            _slidingExpiration = slidingExpiration;
        }

        private void InvokeExpirationCallback(object state)
        {
            var callback = _expirationCallback;

            if (callback != null)
            {
                callback();
            }
        }

        public void SetExpirationCallback(Action callback)
        {
            _expirationCallback = callback;
        }

        public IExpirationToken NewExpirationToken()
        {
            return new ExpirationToken(DateTime.UtcNow + _slidingExpiration);
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}