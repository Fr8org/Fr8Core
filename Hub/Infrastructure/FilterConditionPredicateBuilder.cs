using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Globalization;
using System.Reflection;
using Data.Interfaces.DataTransferObjects;

namespace Hub.Infrastructure
{
    public class FilterConditionPredicateBuilder<T>
    {
        public FilterConditionPredicateBuilder(IEnumerable<FilterConditionDTO> conditions)
        {
            Conditions = conditions;
        }

        public static string ParseConditionToText(List<FilterConditionDTO> filterData)
        {
            var parsedConditions = new List<string>();

            filterData.ForEach(condition =>
            {
                string parsedCondition = condition.Field;

                switch (condition.Operator)
                {
                    case "eq":
                        parsedCondition += " = ";
                        break;
                    case "neq":
                        parsedCondition += " != ";
                        break;
                    case "gt":
                        parsedCondition += " > ";
                        break;
                    case "gte":
                        parsedCondition += " >= ";
                        break;
                    case "lt":
                        parsedCondition += " < ";
                        break;
                    case "lte":
                        parsedCondition += " <= ";
                        break;
                    default:
                        throw new NotSupportedException(string.Format("Not supported operator: {0}", condition.Operator));
                }

                parsedCondition += string.Format("'{0}'", condition.Value);
                parsedConditions.Add(parsedCondition);
            });

            var finalCondition = string.Join(" AND ", parsedConditions);

            return finalCondition;
        }

        public IEnumerable<FilterConditionDTO> Conditions { get; private set; }

        public Expression<Func<T, bool>> ToPredicate()
        {
            var paramExpr = Expression.Parameter(typeof(T));
            var trueExpr = Expression.Constant(true);
            Expression expr = trueExpr;

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
                        expr = Expression.AndAlso(expr, conditionExpr);
                    }
                }
            }

            return Expression.Lambda<Func<T, bool>>(expr, paramExpr);
        }

        private static Expression CreateFieldCondition(
            Expression rootExpr, FieldInfo fieldInfo, FilterConditionDTO condition)
        {
            object constValue;

            if (!ExtractConstantValue(fieldInfo.FieldType, condition.Value, out constValue))
            {
                return null;
            }

            var fieldExpr = Expression.Field(rootExpr, fieldInfo);
            var constExpr = Expression.Constant(constValue);

            Expression result;
            if (!CreateOperatorExpression(fieldExpr, constExpr, condition.Operator, out result))
            {
                return null;
            }

            return result;
        }

        private static Expression CreatePropertyCondition(
            Expression rootExpr, PropertyInfo propInfo, FilterConditionDTO condition)
        {
            object constValue;

            if (!ExtractConstantValue(propInfo.PropertyType, condition.Value, out constValue))
            {
                return null;
            }

            var propExpr = Expression.Property(rootExpr, propInfo);
            var constExpr = Expression.Constant(constValue);

            Expression result;
            if (!CreateOperatorExpression(propExpr, constExpr, condition.Operator, out result))
            {
                return null;
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
            else if (type == typeof(DateTime))
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
                else
                {
                    result = null;
                    return false;
                }
            }
            else
            {
                result = null;
                return false;
            }
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
