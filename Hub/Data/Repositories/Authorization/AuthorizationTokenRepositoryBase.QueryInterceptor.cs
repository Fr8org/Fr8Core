using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Data.Entities;

namespace Data.Repositories
{
    partial class AuthorizationTokenRepositoryBase
    {
        class InterceptedQuery : IOrderedQueryable<AuthorizationTokenDO>
        {
            private readonly Expression _expression;
            private readonly InterceptingProvider _provider;

            public Type ElementType => typeof(AuthorizationTokenDO);
            public Expression Expression => _expression;
            public IQueryProvider Provider => _provider;

            public InterceptedQuery(InterceptingProvider provider, Expression expression)
            {
                _provider = provider;
                _expression = expression;
            }

            public IEnumerator<AuthorizationTokenDO> GetEnumerator()
            {
                return _provider.ExecuteQuery(_expression);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _provider.ExecuteQuery(_expression);
            }
        }
    }
}
