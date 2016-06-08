using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace TerminalSqlUtilities
{
    public class SqlClientSqlQueryBuilder : ISqlQueryBuilder
    {
        private class ConditionGeneratorContext
        {
            private int _paramNum = 0;

            public string GetNextParameterName()
            {
                var paramNum = _paramNum;
                ++_paramNum;

                return "@param" + paramNum.ToString();
            }
        }

        private static readonly Dictionary<string, string> _operatorMap
            = new Dictionary<string, string>()
        {
            { "eq", "=" },
            { "neq", "<>" },
            { "gt", ">" },
            { "gte", ">=" },
            { "lt", "<" },
            { "lte", "<=" }
        };


        public SqlQuery BuildSelectQuery(SelectQuery query)
        {
            var parameters = new List<SqlQueryParameter>();

            var sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append(CreateSelectList(query));
            sb.Append(" FROM ");
            sb.Append(query.TableInfo.ToString());

            if (query.Conditions != null && query.Conditions.Any())
            {
                sb.Append(" WHERE ");

                var whereClause = CreateWhereClause(query);
                sb.Append(whereClause.Sql);
                parameters.AddRange(whereClause.Parameters);
            }

            var sqlQuery = new SqlQuery(sb.ToString(), parameters);
            return sqlQuery;
        }

        private string CreateSelectList(SelectQuery query)
        {
            var sb = new StringBuilder();
            
            foreach (var column in query.Columns)
            {
                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(column.ColumnName);
            }

            return sb.ToString();
        }

        private ParametrizedSqlPart CreateWhereClause(SelectQuery query)
        {
            var context = new ConditionGeneratorContext();
            var columnTypeMap = query.Columns.ToDictionary(x => x.ColumnName, x => x.DbType);

            var sb = new StringBuilder();
            var parameters = new List<SqlQueryParameter>();

            foreach (var condition in query.Conditions)
            {
                if (sb.Length > 0)
                {
                    sb.Append(" AND ");
                }

                var conditionPart = CreateCondition(condition, columnTypeMap, context);
                sb.Append(conditionPart.Sql);
                parameters.AddRange(conditionPart.Parameters);
            }

            var sqlPart = new ParametrizedSqlPart()
            {
                Sql = sb.ToString(),
                Parameters = parameters
            };

            return sqlPart;
        }

        private ParametrizedSqlPart CreateCondition(
            FilterConditionDTO condition,
            IDictionary<string, DbType> columnTypeMap,
            ConditionGeneratorContext context)
        {
            DbType dbType;

            if (!columnTypeMap.TryGetValue(condition.Field, out dbType))
            {
                throw new ApplicationException("Invalid column name.");
            }

            switch (dbType)
            {
                case DbType.Boolean:
                    return CreateBooleanCondition(condition, context);

                case DbType.String:
                    return CreateStringCondition(condition, context);

                case DbType.Int32:
                    return CreateInt32Condition(condition, context);

                default:
                    throw new ApplicationException("Invalid column data-type.");
            }
        }

        private string GetSqlOperator(string conditionOperator)
        {
            string sqlOperator;
            if (!_operatorMap.TryGetValue(conditionOperator, out sqlOperator))
            {
                throw new ApplicationException("Invalid condition operator");
            }

            return sqlOperator;
        }

        private ParametrizedSqlPart CreateConditionWithValue(
            FilterConditionDTO condition,
            object value,
            ConditionGeneratorContext context)
        {
            var paramName = context.GetNextParameterName();

            var sb = new StringBuilder();
            sb.Append("(");
            sb.Append("[");
            sb.Append(condition.Field);
            sb.Append("]");
            sb.Append(" ");
            sb.Append(GetSqlOperator(condition.Operator));
            sb.Append(" ");
            sb.Append(paramName);
            sb.Append(")");

            var sqlPart = new ParametrizedSqlPart()
            {
                Sql = sb.ToString(),
                Parameters = new List<SqlQueryParameter>()
                {
                    new SqlQueryParameter(paramName, value != null ? value : DBNull.Value)
                }
            };

            return sqlPart;
        }

        private ParametrizedSqlPart CreateBooleanCondition(
            FilterConditionDTO condition,
            ConditionGeneratorContext context)
        {
            var booleanValue = string.Equals(
                condition.Value,
                "true",
                StringComparison.InvariantCultureIgnoreCase
            );

            return CreateConditionWithValue(condition, booleanValue, context);
        }

        private ParametrizedSqlPart CreateStringCondition(
            FilterConditionDTO condition,
            ConditionGeneratorContext context)
        {
            return CreateConditionWithValue(condition, condition.Value, context);
        }

        private ParametrizedSqlPart CreateInt32Condition(
            FilterConditionDTO condition,
            ConditionGeneratorContext context)
        {
            int intValue;
            if (!Int32.TryParse(condition.Value, out intValue))
            {
                intValue = 0;
            }

            return CreateConditionWithValue(condition, intValue, context);
        }
    }
}
