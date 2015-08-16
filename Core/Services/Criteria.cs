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
using Data.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Core.Services
{
    public class Criteria : ICriteria
    {
        public bool Evaluate(string criteria, int processId, string envelopeId, IEnumerable<EnvelopeDataDO> envelopeData)
        {
            return Filter(criteria, processId, envelopeId, envelopeData.AsQueryable()).Any();
        }

        public IQueryable<EnvelopeDataDO> Filter(string criteria, int processId, string envelopeId,
            IQueryable<EnvelopeDataDO> envelopeData)
        {
            EventManager.CriteriaEvaluationStarted(processId);
            var filterExpression = ParseCriteriaExpression(criteria, envelopeData);
            IQueryable<EnvelopeDataDO> results =
                envelopeData.Provider.CreateQuery<EnvelopeDataDO>(filterExpression);
            EventManager.CriteriaEvaluationFinished(processId);
            return results;
        }

        private Expression ParseCriteriaExpression<T>(string criteria, IQueryable<T> queryableData)
        {
            Expression criteriaExpression = null;
            ParameterExpression pe = Expression.Parameter(typeof(T), "p");
            JObject jCriteria = JObject.Parse(criteria);
            JArray jCriterions = (JArray)jCriteria.Property("criteria").Value;
            foreach (var jCriterion in jCriterions.OfType<JObject>())
            {
                var propName = (string)jCriterion.Property("field").Value;
                var op = (string)jCriterion.Property("operator").Value;
                var value = (string)jCriterion.Property("value").Value;
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

            if (criteriaExpression == null)
                criteriaExpression = Expression.Constant(true);

            var whereCallExpression = Expression.Call(
                typeof(Queryable),
                "Where",
                new[] { typeof(T) },
                queryableData.Expression,
                Expression.Lambda<Func<T, bool>>(criteriaExpression, new[] { pe }));
            return whereCallExpression;
        }
    }
}
