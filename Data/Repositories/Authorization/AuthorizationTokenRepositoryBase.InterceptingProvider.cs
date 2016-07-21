using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Data.Entities;

namespace Data.Repositories
{
    partial class AuthorizationTokenRepositoryBase
    {
        class EnumeratorWrapper : IEnumerator<AuthorizationTokenDO>
        {
            private readonly IEnumerator<AuthorizationTokenDO> _underlyingIEnumerator;
            private readonly AuthorizationTokenRepositoryBase _authorizationTokenRepository;

            public AuthorizationTokenDO Current { get; private set; }
            object IEnumerator.Current => Current;
            
            public EnumeratorWrapper(IEnumerator<AuthorizationTokenDO> underlyingIEnumerator, AuthorizationTokenRepositoryBase authorizationTokenRepository)
            {
                _underlyingIEnumerator = underlyingIEnumerator;
                _authorizationTokenRepository = authorizationTokenRepository;
            }

            public bool MoveNext()
            {
                if (_underlyingIEnumerator.MoveNext())
                {
                    Current = _authorizationTokenRepository.Sync(_underlyingIEnumerator.Current);
                    return true;
                }

                return false;
            }

            public void Reset()
            {
                _underlyingIEnumerator.Reset();
                Current = null;
            }
            
            public void Dispose()
            {
                _underlyingIEnumerator.Dispose();
            }
        }

        class InterceptingProvider : IQueryProvider
        {
            private readonly IQueryProvider _underlyingProvider;
            private readonly AuthorizationTokenRepositoryBase _authorizationTokenRepository;

            private InterceptingProvider(IQueryProvider underlyingQueryProvider, AuthorizationTokenRepositoryBase authorizationTokenRepository)
            {
                _underlyingProvider = underlyingQueryProvider;
                _authorizationTokenRepository = authorizationTokenRepository;
            }

            public static IQueryable<AuthorizationTokenDO> Intercept(IQueryable<AuthorizationTokenDO> underlyingQuery, AuthorizationTokenRepositoryBase authorizationTokenRepository)
            {
                InterceptingProvider provider = new InterceptingProvider(underlyingQuery.Provider, authorizationTokenRepository);

                return provider.CreateQuery<AuthorizationTokenDO>(underlyingQuery.Expression);
            }

            public IEnumerator<AuthorizationTokenDO> ExecuteQuery(Expression expression)
            {
                return new EnumeratorWrapper(_underlyingProvider.CreateQuery<AuthorizationTokenDO>(expression).GetEnumerator(), _authorizationTokenRepository);
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                if (typeof(TElement) == typeof(AuthorizationTokenDO))
                {
                    return (IQueryable<TElement>) new InterceptedQuery(this, expression);
                }

                return _underlyingProvider.CreateQuery<TElement>(expression);
            }

            public IQueryable CreateQuery(Expression expression)
            {
                if (typeof(IEnumerable<AuthorizationTokenDO>).IsAssignableFrom(expression.Type))
                {
                    return new InterceptedQuery(this, expression);
                }

                return _underlyingProvider.CreateQuery(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                var result = _underlyingProvider.Execute<TResult>(expression);
                return (TResult) InterceptElement(result);
            }
            
            public object Execute(Expression expression)
            {
                var result = _underlyingProvider.Execute(expression);

                return InterceptElement(result);
            }

            private object InterceptElement(object element)
            {
                var token = element as AuthorizationTokenDO;

                if (token != null)
                {
                    return _authorizationTokenRepository.Sync(token);
                }

                return element;
            }
        }
    }
}
