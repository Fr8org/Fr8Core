using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hub.Infrastructure
{
    partial class AsyncMultiLock
    {
        private class LockScope : IDisposable
        {
            private readonly AsyncMultiLock _queue;
            private readonly TaskCompletionSource<IDisposable> _wait;
            private int _isDisposed;

            public readonly object Token;
            public Task Task => _wait.Task;

            public LockScope(AsyncMultiLock queue, object token)
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
