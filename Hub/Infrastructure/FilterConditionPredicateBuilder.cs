using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Globalization;
using System.Reflection;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Hub.Infrastructure
{
    public class FilterConditionPredicateBuilder<T>
    {
        public FilterConditionPredicateBuilder(IEnumerable<FilterConditionDTO> conditions)
        {
            Conditions = conditions;
        }

        public IEnumerable<FilterConditionDTO> Conditions { get; private set; }

        public Expression<Func<T, bool>> ToPredicate()
        {
            var paramExpr = Expression.Parameter(typeof(T));
            Expression expr = null;

            if (Conditions.Any())
            {
                foreach (var condition in Conditions)
                {
                    var fieldInfo = typeof(T).GetField(condition.Field);
                    var propInfo = typeof(T).GetProperty(condition.Field);

                    if (fieldInfo == null && propInfo == null)
                    {
                        continue;
                    }

                    Expression conditionExpr;
                    if (fieldInfo != null)
                    {
                        conditionExpr = CreateFieldCondition(paramExpr, fieldInfo, condition);
                    }
                    else
                    {
                        conditionExpr = CreatePropertyCondition(paramExpr, propInfo, condition);
                    }

                    if (conditionExpr != null)
                    {
                        expr = expr == null ? conditionExpr : Expression.AndAlso(expr, conditionExpr);
                    }
                }

                return Expression.Lambda<Func<T, bool>>(expr ?? Expression.Constant(false), paramExpr);
            }

            return Expression.Lambda<Func<T, bool>>(Expression.Constant(true), paramExpr);
        }

        private static bool TryGetNullableType(Type orignalType, out Type underlyingType)
        {
            if (orignalType.IsGenericType && orignalType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                underlyingType = orignalType.GetGenericArguments()[0];
                return true;
            }

            underlyingType = orignalType;
            return false;
        }

        private static Expression CreateFieldCondition(
            Expression rootExpr, FieldInfo fieldInfo, FilterConditionDTO condition)
        {
            object constValue;
            Type underlyingType = fieldInfo.FieldType;
            var isNullable = TryGetNullableType(underlyingType, out underlyingType);

            if (!ExtractConstantValue(underlyingType, condition.Value, out constValue))
            {
                return null;
            }

            var fieldExpr = (Expression)Expression.Field(rootExpr, fieldInfo);

            if (isNullable)
            {
                fieldExpr = Expression.Property(fieldExpr, "Value");
            }

            var constExpr = Expression.Constant(constValue);

            Expression result;
            if (!CreateOperatorExpression(fieldExpr, constExpr, condition.Operator, out result))
            {
                return null;
            }

            if (isNullable)
            {
                result = Expression.AndAlso(Expression.NotEqual(Expression.Field(rootExpr, fieldInfo), Expression.Constant(null)), result);
            }

            return result;
        }

        private static Expression CreatePropertyCondition(
            Expression rootExpr, PropertyInfo propInfo, FilterConditionDTO condition)
        {
            object constValue;
            Type underlyingType = propInfo.PropertyType;
            var isNullable = TryGetNullableType(underlyingType, out underlyingType);

            if (!ExtractConstantValue(underlyingType, condition.Value, out constValue))
            {
                return null;
            }

            var propExpr = (Expression)Expression.Property(rootExpr, propInfo);
            var constExpr = Expression.Constant(constValue);

            if (isNullable)
            {
                propExpr = Expression.Property(propExpr, "Value");
            }

            Expression result;
            if (!CreateOperatorExpression(propExpr, constExpr, condition.Operator, out result))
            {
                return null;
            }

            if (isNullable)
            {
                result = Expression.AndAlso(Expression.NotEqual(Expression.Property(rootExpr, propInfo), Expression.Constant(null)), result);
            }

            return result;
        }

        private static bool ExtractConstantValue(Type type, string value, out object result)
        {
            if (type == typeof(string))
            {
                result = value;
                return true;
            }

            if (type == typeof(DateTime))
            {
                if (string.IsNullOrEmpty(value))
                {
                    result = null;
                    return false;
                }

                DateTime tempDateTime;
                if (DateTime.TryParseExact(value, "dd-MM-yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out tempDateTime))
                {
                    result = tempDateTime;
                    return true;
                }

                result = null;
                return false;
            }

            result = null;
            return false;
        }

        private static bool CreateOperatorExpression(
            Expression propExpr, Expression constExpr, string condition, out Expression result)
        {
            result = null;

            switch (condition)
            {
                case "gt":
                    result = Expression.GreaterThan(propExpr, constExpr);
                    break;
                case "gte":
                    result = Expression.GreaterThanOrEqual(propExpr, constExpr);
                    break;
                case "lt":
                    result = Expression.LessThan(propExpr, constExpr);
                    break;
                case "lte":
                    result = Expression.LessThanOrEqual(propExpr, constExpr);
                    break;
                case "eq":
                    result = Expression.Equal(propExpr, constExpr);
                    break;
                case "neq":
                    result = Expression.NotEqual(propExpr, constExpr);
                    break;
            }

            return result != null;
        }
    }
}
