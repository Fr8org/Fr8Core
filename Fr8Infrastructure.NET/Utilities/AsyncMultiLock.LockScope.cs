using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fr8.Infrastructure.Utilities
{
    partial class AsyncMultiLock
    {
        private class LockScope : IDisposable
        {
            private readonly Fr8.Infrastructure.Utilities.AsyncMultiLock _queue;
            private readonly TaskCompletionSource<IDisposable> _wait;
            private int _isDisposed;

            public readonly object Token;
            public Task Task => _wait.Task;

            public LockScope(Fr8.Infrastructure.Utilities.AsyncMultiLock queue, object token)
            {
                _queue = queue;
                Token = token;
                _wait = new TaskCompletionSource<IDisposable>(this);
            }

            public void Dispose()
            {
                if (Interlocked.Exchange(ref _isDisposed, 1) == 1)
                {
                    return;
                }

                _wait.SetResult(this);
                _queue.Release(this);
            }
        }
    }
}
