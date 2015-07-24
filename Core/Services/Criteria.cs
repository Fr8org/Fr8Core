using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Interfaces;
using Data.Entities;
using Newtonsoft.Json.Linq;

namespace Core.Services
{
    public class Criteria : ICriteria
    {
        public bool Evaluate(string criteria, int processId, string envelopeId, IEnumerable<EnvelopeDataDO> envelopeData)
        {
            return Filter(criteria, processId, envelopeId, envelopeData).Any();
        }

        public IEnumerable<EnvelopeDataDO> Filter(string criteria, int processId, string envelopeId,
            IEnumerable<EnvelopeDataDO> envelopeData)
        {
            var filterExpression = ParseCriteriaExpression(criteria);
            IQueryable<EnvelopeDataDO> results =
                envelopeData.AsQueryable().Provider.CreateQuery<EnvelopeDataDO>(filterExpression);
            return results;
        }

        private Expression ParseCriteriaExpression(string criteria)
        {
            Expression criteriaExpression = null;
            ParameterExpression pe = Expression.Parameter(typeof(string), "p");
            JObject jCriteria = JObject.Parse(criteria);
            JArray jCriterions = (JArray)jCriteria.Property("criteria").Value;
            foreach (var jCriterion in jCriterions.OfType<JObject>())
            {
                var propName = jCriterion.Property("field").Value<string>();
                var op = jCriterion.Property("operator").Value<string>();
                var value = jCriterion.Property("value").Value<string>();
                Expression left = Expression.Property(pe, propName);
                Expression right = Expression.Constant(value);
                Expression criterionExpression;
                switch (op)
                {
                    case "Equals":
                        criterionExpression = Expression.Equal(left, right);
                        break;
                    case "GreaterThan":
                        criterionExpression = Expression.GreaterThan(left, right);
                        break;
                    case "GreaterThanOrEquals":
                        criterionExpression = Expression.GreaterThanOrEqual(left, right);
                        break;
                    case "LessThan":
                        criterionExpression = Expression.LessThan(left, right);
                        break;
                    case "LessThanOrEquals":
                        criterionExpression = Expression.LessThanOrEqual(left, right);
                        break;
                    default:
                        throw new NotSupportedException(string.Format("Not supported operator: {0}", op));
                }

                if (criteriaExpression == null)
                    criteriaExpression = criterionExpression;
                else
                    criteriaExpression = Expression.AndAlso(criteriaExpression, criterionExpression);
            }
            return criteriaExpression;
        }
    }
}
