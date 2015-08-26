using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Core.Interfaces;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using DocuSign.Integrations.Client;
using Newtonsoft.Json.Linq;
using StructureMap;

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
			if (criteria == null)
				throw new ArgumentNullException("criteria");
			if (criteria == string.Empty)
				throw new ArgumentException("criteria is empty", "criteria");
			if (envelopeData == null)
				throw new ArgumentNullException("envelopeData");

			return Filter(criteria, processId, envelopeData.AsQueryable()).Any();
		}
		public bool Evaluate(List<EnvelopeDataDTO> envelopeData, ProcessNodeDO curProcessNode)
		{
			if (envelopeData == null)
				throw new ArgumentNullException("envelopeData");
			if (curProcessNode == null)
				throw new ArgumentNullException("curProcessNode");

			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				var curCriteria = uow.CriteriaRepository.FindOne(c => c.ProcessNodeTemplate.Id == curProcessNode.Id);
				if (curCriteria == null)
					throw new ApplicationException("failed to find expected CriteriaDO while evaluating ProcessNode");
				if (curCriteria.CriteriaExecutionType == CriteriaExecutionType.WithoutConditions)
					return true;
				else
					return Evaluate(curCriteria.ConditionsJSON, curProcessNode.Id, envelopeData);
			}
		}
		public IQueryable<EnvelopeDataDTO> Filter(string criteria, int processId,
			 IQueryable<EnvelopeDataDTO> envelopeData)
		{
			if (criteria == null)
				throw new ArgumentNullException("criteria");
			if (criteria == string.Empty)
				throw new ArgumentException("criteria is empty", "criteria");
			if (envelopeData == null)
				throw new ArgumentNullException("envelopeData");

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
				var propInfo = typeof(T).GetProperty(propName);
				var op = (string)jCriterion.Property("operator").Value;
				var value = ((JValue)jCriterion.Value<object>("value")).ToObject(propInfo.PropertyType);
				Expression left = Expression.Property(pe, propInfo);
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
