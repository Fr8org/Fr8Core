using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Interfaces;
using Data.Entities;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces;

namespace DockyardTest.Services
{
	[TestFixture]
	[Category("CriteriaService")]
	public class CriteriaServiceTests : BaseTest
	{
		private ICriteria _criteria;

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			_criteria = ObjectFactory.GetInstance<ICriteria>();
		}

		[Test, Ignore]
		public void CriteriaService_CanApplyEqualCriterion()
		{
			var envelopeDataList = FixtureData.TestEnvelopeDataList1();
			var results = new bool[2];
			var values = new string[] { envelopeDataList.First().Value, "__non-existing-value__" };
			for (var i = 0; i < 2; i++)
			{
				var criteriaObject = new { criteria = new[] { new { field = "Value", @operator = "Equals", value = values[i] } } };
				var criteriaString = JsonConvert.SerializeObject(criteriaObject);
				results[i] = _criteria.Evaluate(criteriaString, 0, envelopeDataList);
			}
			Assert.IsTrue(results[0], "Criteria#Evaluate returned incorrect value.");
			Assert.IsFalse(results[1], "Criteria#Evaluate returned incorrect value.");
		}
		[Test]
		[ExpectedException(typeof(ArgumentNullException), ExpectedMessage="criteria")]
		public void Evaluate_CriteriaIsNull_ExpectedArgumentNullException()
		{
			var envelopeDataList = FixtureData.TestEnvelopeDataList1();

			_criteria.Evaluate(null, 1, envelopeDataList);
		}
		[Test]
		[ExpectedException(typeof(ArgumentException), ExpectedMessage = "criteria")]
		public void Evaluate_CriteriaIsEmtpy_ExpectedArgumentException()
		{
			var envelopeDataList = FixtureData.TestEnvelopeDataList1();

			_criteria.Evaluate(string.Empty, 1, envelopeDataList);
		}
		[Test]
		[ExpectedException(typeof(JsonReaderException))]
		public void Evaluate_CriteriaIsInvalidJSON_ExpectedJsonReaderException()
		{
			var envelopeDataList = FixtureData.TestEnvelopeDataList1();

			_criteria.Evaluate("THIS_IS_NOT_CORRECT_JSON_DATA", 1, envelopeDataList);
		}
		[Test]
		[ExpectedException(typeof(ArgumentNullException), ExpectedMessage = "envelopeData")]
		public void Evaluate_EnvelopeDataIsNull_ExpectedArgumentNullException()
		{
			var criteria = FixtureData.TestCriteria1();

			_criteria.Evaluate(criteria.ConditionsJSON, 1, null);
		}
		[Test]
		public void Evaluate_EnvelopeDataIsEmtpy_OK()
		{
			var criteria = FixtureData.TestCriteria1();
			var envelopeDataList = new EnvelopeDataDTO[0];

			_criteria.Evaluate(criteria.ConditionsJSON, 1, envelopeDataList);
		}


		[Test]
		public void Evaluate_CurEnvelopeIsNull_Failed()
		{
			var processNode = FixtureData.TestProcessNode1();
			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				uow.CriteriaRepository.Add(FixtureData.TestCriteria1());
				uow.SaveChanges();
			}

			bool gotException = false;
			try { _criteria.Evaluate(null, processNode); }
			catch (Exception ex)
			{
				gotException = true;
			}

			Assert.AreEqual(gotException, true, "Argument 'curEnvelope' was null. Expected any exception!");
		}
		[Test]
		[ExpectedException(typeof(ArgumentNullException), ExpectedMessage = "curProcessNode")]
		public void Evaluate_CurProcessNodeIsNull_ExpectedArgumentNullException()
		{
			var envelope = FixtureData.TestEnvelope1();
			_criteria.Evaluate(envelope, null);
		}
		[Test]
		[ExpectedException(typeof(ApplicationException), ExpectedMessage = "failed to find expected CriteriaDO while evaluating ProcessNode")]
		public void Evaluate_ProcessNodeDoesntHaveCriteria_ExpectedApplicationException()
		{
			var processNode = FixtureData.TestProcessNode1();
			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				var c = FixtureData.TestCriteria1();
				c.ProcessNodeTemplate.Id = processNode.Id + 1;
				uow.CriteriaRepository.Add(c);
				uow.SaveChanges();
			}
			var envelope = FixtureData.TestEnvelope1();

			_criteria.Evaluate(envelope, processNode);
		}
	}
}
