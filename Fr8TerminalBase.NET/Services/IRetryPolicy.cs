using System;
using System.Threading.Tasks;

namespace Fr8.TerminalBase.Services
{
    public interface IRetryPolicy
    {
        Task<TResult> Do<TResult>(Func<Task<TResult>> action);
    }

    public class SingleRunRetryPolicy : IRetryPolicy
    {
        public Task<TResult> Do<TResult>(Func<Task<TResult>> action)
        {
            return action();
        }
    }
    
    public class SimpleRetryPolicy : IRetryPolicy
    {
        private readonly int _retryCount;
        private readonly int _timeout;

        public SimpleRetryPolicy(int retryCount, int timeout)
        {
            _retryCount = retryCount;
            _timeout = timeout;
        }

        public async Task<TResult> Do<TResult>(Func<Task<TResult>> action)
        {
            Exception lastException;

            for (int i = 0; ; i++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }

                if (i >= _retryCount - 1)
                {
                    break;
                }

                if (_timeout > 0)
                {
                    await Task.Delay(_timeout);
                }
            }

            throw lastException;
        }
    }
}
