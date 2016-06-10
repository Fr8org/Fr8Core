using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Hub.Infrastructure
{
    public partial class AsyncMultiLock
    {
        private readonly Dictionary<object, LockScope> _tails = new Dictionary<object, LockScope>();
        private readonly object _sync = new object();

        public async Task<IDisposable> Lock(object token)
        {
            Debug.WriteLine($"Aquiring lock for {token}");
            var scope = new LockScope(this, token);
            LockScope waitScope;

            lock (_sync)
            {
                LockScope tail;

                // if there is no currently executing tasks create new scope and set is as currently executing task
                // we will immediately return current scope, because we do not have to wait anyone
                if (!_tails.TryGetValue(token, out tail))
                {
                    Debug.WriteLine($"Immediately aquired lock for {token}");
                    _tails[token] = scope;
                    return scope;
                }

                // else we will wait for last executed task to complete
                waitScope = tail;
                _tails[token] = scope;
            }
            Debug.WriteLine($"Waiting for lock for {token}");
            // and wait for that task
            await waitScope.Task;
            Debug.WriteLine($"Aquiring lock for {token} after waiting");
            return scope;
        }

        private void Release(LockScope lockScope)
        {
            lock (_sync)
            {
                // we are the last task in the queue.
                LockScope tail;
                if (_tails.TryGetValue(lockScope.Token, out tail) && tail == lockScope)
                {
                    Debug.WriteLine($"Releasing lock for {lockScope.Token}");
                    _tails.Remove(lockScope.Token);
                }
            }
        }
    }
}
