using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fr8.Infrastructure.Utilities
{
    public partial class AsyncMultiLock
    {
        private readonly Dictionary<object, AsyncMultiLock.LockScope> _tails = new Dictionary<object, AsyncMultiLock.LockScope>();
        private readonly object _sync = new object();

        public async Task<IDisposable> Lock(object token)
        {
            var scope = new AsyncMultiLock.LockScope(this, token);
            AsyncMultiLock.LockScope waitScope;

            lock (_sync)
            {
                AsyncMultiLock.LockScope tail;

                // if there is no currently executing tasks create new scope and set is as currently executing task
                // we will immediately return current scope, because we do not have to wait anyone
                if (!_tails.TryGetValue(token, out tail))
                {
                    _tails[token] = scope;
                    return scope;
                }

                // else we will wait for last executed task to complete
                waitScope = tail;
                _tails[token] = scope;
            }

            // and wait for that task
            await waitScope.Task;

            return scope;
        }

        private void Release(AsyncMultiLock.LockScope lockScope)
        {
            lock (_sync)
            {
                // we are the last task in the queue.
                AsyncMultiLock.LockScope tail;
                if (_tails.TryGetValue(lockScope.Token, out tail) && tail == lockScope)
                {
                    _tails.Remove(lockScope.Token);
                }
            }
        }
    }
}
