using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.Repositories;

namespace TerminalBase.Services
{
    static partial class MTSearchHelper
    {
        // This class can be used to query MT DB using criterias from Query Builder control.
        // We need a way to query MT DB using manifest type that is unknown at the run-time. 
        // We want to use reflection as little as possible. So we create generic class for converting Query Builder filters and will create instance if this class using reflection. To access members of this class we will use non-generic interface.
        // Se creating new instance will be the only place reflection is used.
        private class MtQueryProvider<T> : IMtQueryProvider
            where  T : Manifest
        {
            public Type Type
            {
                get { return typeof (T); }
            }

            public List<object> Query(IUnitOfWork uow, string accountId, List<FilterConditionDTO> conditions)
            {
                var queryable = uow.MultiTenantObjectRepository.AsQueryable<T>(uow, accountId);
                var result = new List<object>();

                 result.AddRange(CriteriaToMtQuery(conditions, queryable));

                return result;
            }

            private static IMtQueryable<T> CriteriaToMtQuery(List<FilterConditionDTO> conditions, IMtQueryable<T> queryable)
            {
                var type = typeof (T);
                ParameterExpression param = Expression.Parameter(type, "x");

                foreach (var condition in conditions)
                {
                    var fieldName = condition.Field;
                    MemberInfo member;
                    var fieldType = GetMemberType(type, fieldName, out member);

                    if (fieldType == null)
                    {
                        continue;
                    }

                    Expression queryExpression = null;
                    object convertedValue;
                    Expression accessor;
                    
                    if (TryConvert(fieldType, condition.Value, out convertedValue))
                    {
                        accessor = Expression.MakeMemberAccess(param, member);
                    }
                    else
                    {
                        continue;
                    }
                   
                    var operand = Expression.Constant(convertedValue);

                    switch (condition.Operator)
                    {
                        case "eq":
                            queryExpression = Expression.Equal(accessor, operand);
                            break;

                        case "neq":
                            queryExpression = Expression.NotEqual(accessor, operand);
                            break;

                        case "gt":
                            queryExpression = Expression.GreaterThan(accessor, operand);
                            break;

                        case "gte":
                            queryExpression = Expression.GreaterThanOrEqual(accessor, operand);
                            break;

                        case "lte":
                            queryExpression = Expression.LessThanOrEqual(accessor, operand);
                            break;

                        case "lt":
                            queryExpression = Expression.LessThan(accessor, operand);
                            break;
                    }

                    if (queryExpression == null)
                    {
                        continue;
                    }

                    queryable = queryable.Where(Expression.Lambda<Func<T, bool>>(queryExpression, param));
                }

                return queryable;
            }

            private static Type GetMemberType(Type type, string memberName, out MemberInfo memberInfo)
            {
                var field = type.GetField(memberName);

                if (field != null)
                {
                    memberInfo = field;
                    return field.FieldType;
                }

                var prop = type.GetProperty(memberName);
                if (prop != null)
                {
                    memberInfo = prop;
                    return prop.PropertyType;
                }

                memberInfo = null;
                return null;
            }

            private static bool TryConvert(Type targetType, string value, out object convertedValue)
            {
                if (targetType == typeof(string))
                {
                    convertedValue = value;
                    return true;
                }

                try
                {
                    convertedValue = Convert.ChangeType(value, targetType);
                    return true;
                }
                catch (Exception)
                {
                    convertedValue = null;
                    return false;
                }
            }
        }
    }
}