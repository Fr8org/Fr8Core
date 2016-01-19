using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Data.Expressions;
using Data.Interfaces;
using Data.Interfaces.Manifests;

namespace Data.Repositories
{
    public class MtQueryExecutor<T> : IMtQueryExecutor<T>
        where T : Manifest
    {
        private readonly IMultiTenantObjectRepository _mtObjectRepository;
        private readonly string _currentAccountId;
        private readonly IUnitOfWork _uow;

        public MtQueryExecutor(IMultiTenantObjectRepository mtObjectRepository, IUnitOfWork uow, string currentAccountId)
        {
            _mtObjectRepository = mtObjectRepository;
            _currentAccountId = currentAccountId;
            _uow = uow;
        }

        // Currently we support only string of MtWhere. All Expressions from MtWhere are combined using 'and' into one expression.
        public IEnumerator<T> RunQuery(IMtQueryable<T> queryable) 
        {
            IMtQueryable<T> root = queryable;
            var condtions = new List<Expression<Func<T, bool>>>();

            while (root.Previous != null)
            {
                if (root is MtQueryAll<T>)
                {
                    break;
                }

                if (root is MtWhere<T>)
                {
                    condtions.Add(((MtWhere<T>)root).Condition);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Query part {0} is not supported", root.GetType().Name));
                }

                root = root.Previous;
            }

            Expression<Func<T, bool>> result;

            if (condtions.Count == 0)
            {
                result = x => true;
            }
            else if (condtions.Count == 1)
            {
                result = condtions[0];
            }
            else
            {
                var sourceParameter = condtions[0].Parameters[0];
                var temp = ReplaceParameter(condtions[condtions.Count - 1], sourceParameter);

                for (int i = condtions.Count - 2; i >= 0; i--)
                {
                    temp = Expression.AndAlso(temp, ReplaceParameter(condtions[i], sourceParameter));
                }

                result = Expression.Lambda<Func<T, bool>>(temp, sourceParameter);
            }

            return _mtObjectRepository.Query(_uow, _currentAccountId, result).GetEnumerator();
        }

        private static Expression ReplaceParameter(Expression<Func<T, bool>> expr, ParameterExpression sourceParameter)
        {
            return new ReplaceParameterVisitor(expr.Parameters[0], sourceParameter).Visit(expr.Body);
        }
    }
}