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
using System.Linq;
using System.Net;
using System.Net.Http;
using StructureMap;
using AutoMapper;
using Core.Interfaces;
using Data.Entities;
using Data.Infrastructure.AutoMapper;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Utilities;
using Data.Wrappers;

namespace Core.Services
{
    public class Criteria : ICriteria
    {
        private IEnvelope _envelope;
        public Criteria()
        {
            _envelope = ObjectFactory.GetInstance<IEnvelope>();
        }
        public bool Evaluate(string criteria, int processId, IEnumerable<EnvelopeDataDTO> envelopeData)
        {
            return Filter(criteria, processId, envelopeData.AsQueryable()).Any();
        }

        public bool Evaluate(EnvelopeDO curEnvelope, ProcessNodeDO curProcessNode)
        {
            
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        {
                var curCriteria = uow.CriteriaRepository.FindOne(c => c.ProcessNodeTemplate.Id == curProcessNode.Id);
                if (curCriteria == null)
                    throw new ApplicationException("failed to find expected CriteriaDO while evaluating ProcessNode");

                DocuSign.Integrations.Client.Envelope curDocuSignEnvelope = new DocuSign.Integrations.Client.Envelope(); //should just change GetEnvelopeData to pass an EnvelopeDO


                return Evaluate(curCriteria.ConditionsJSON, curProcessNode.Id, _envelope.GetEnvelopeData(curDocuSignEnvelope));
            };
        }


        public IQueryable<EnvelopeDataDTO> Filter(string criteria, int processId, 
            IQueryable<EnvelopeDataDTO> envelopeData)
        {
            EventManager.CriteriaEvaluationStarted(processId);
            var filterExpression = ParseCriteriaExpression(criteria, envelopeData);
            IQueryable<EnvelopeDataDTO> results =
                envelopeData.Provider.CreateQuery<EnvelopeDataDTO>(filterExpression);
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
