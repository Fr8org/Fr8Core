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
using System.Linq.Expressions;
using Utilities;

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

		[Test]
		public void Evaluate_CriteriaIsNull_ExpectedArgumentNullException()
		{
			var envelopeDataList = FixtureData.TestEnvelopeDataList1();

			var ex = Assert.Throws<ArgumentNullException>(() => _criteria.Evaluate(null, 1, envelopeDataList));

			Assert.AreEqual("criteria", ex.ParamName);
		}
		[Test]
		public void Evaluate_CriteriaIsEmtpy_ExpectedArgumentException()
		{
			var envelopeDataList = FixtureData.TestEnvelopeDataList1();

			var ex = Assert.Throws<ArgumentException>(() => _criteria.Evaluate(string.Empty, 1, envelopeDataList));

			Assert.AreEqual("criteria", ex.ParamName);
		}
		[Test]
		[ExpectedException(typeof(JsonReaderException))]
		public void Evaluate_CriteriaIsInvalidJSON_ExpectedJsonReaderException()
		{
			var envelopeDataList = FixtureData.TestEnvelopeDataList1();

			_criteria.Evaluate("THIS_IS_NOT_CORRECT_JSON_DATA", 1, envelopeDataList);
		}
		[Test]
		public void Evaluate_EnvelopeDataIsNull_ExpectedArgumentNullException()
		{
			var criteria = FixtureData.TestCriteria1();


			var ex = Assert.Throws<ArgumentNullException>(() => _criteria.Evaluate(criteria.ConditionsJSON, 1, null));

			Assert.AreEqual("envelopeData", ex.ParamName);
		}
		[Test]
		public void Evaluate_EnvelopeDataIsEmtpy_OK()
		{
			var criteria = FixtureData.TestCriteria1();
			var envelopeDataList = new EnvelopeDataDTO[0];

			_criteria.Evaluate(criteria.ConditionsJSON, 1, envelopeDataList);
		}


		[Test]
		public void Evaluate_EnvelopeDataIsNull_Failed()
		{
			// Create valid data
			var processNode = FixtureData.TestProcessNode1();
			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				uow.CriteriaRepository.Add(FixtureData.TestCriteria1());
				uow.SaveChanges();
			}

			var ex = Assert.Throws<ArgumentNullException>(() => _criteria.Evaluate(null, processNode));

			Assert.AreEqual("envelopeData", ex.ParamName);
		}
		[Test]
		public void Evaluate_CurProcessNodeIsNull_ExpectedArgumentNullException()
		{
			var envelopeData = FixtureData.TestEnvelopeDataList1();

			var ex = Assert.Throws<ArgumentNullException>(() => _criteria.Evaluate(envelopeData, null));

			Assert.AreEqual("curProcessNode", ex.ParamName);
		}
		[Test]
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
			var envelopeData = FixtureData.TestEnvelopeDataList1();
			// I'm not sure that ApplicationException is good type of exception in this situation
			var ex = Assert.Throws<ApplicationException>(() => _criteria.Evaluate(envelopeData, processNode));

			Assert.AreEqual("failed to find expected CriteriaDO while evaluating ProcessNode", ex.Message);
		}

		[Test]
		public void Filter_CriteriaIsNull_ExpectedArgumentNullException()
		{
			var envelopeDataList = FixtureData.TestEnvelopeDataList1().AsQueryable();

			var ex = Assert.Throws<ArgumentNullException>(() => _criteria.Filter(null, 1, envelopeDataList));

			Assert.AreEqual("criteria", ex.ParamName);
		}
		[Test]
		public void Filter_CriteriaIsEmpty_ExpectedArgumentException()
		{
			var envelopeDataList = FixtureData.TestEnvelopeDataList1().AsQueryable();

			var ex = Assert.Throws<ArgumentException>(() => _criteria.Filter(string.Empty, 1, envelopeDataList));

			Assert.AreEqual("criteria", ex.ParamName);
		}
		[Test]
		[ExpectedException(typeof(JsonReaderException))]
		public void Filter_CriteriaIsInvalidJSON_ExpectedJsonReaderException()
		{
			var envelopeDataList = FixtureData.TestEnvelopeDataList1().AsQueryable();

			_criteria.Filter("THIS_IS_NOT_CORRECT_JSON_DATA", 1, envelopeDataList);
		}
		[Test]
		public void Filter_EnvelopeDataIsNull_ExpectedArgumentNullException()
		{
			var criteria = FixtureData.TestCriteria1();

			var ex = Assert.Throws<ArgumentNullException>(() => _criteria.Filter(criteria.ConditionsJSON, 1, null));

			Assert.AreEqual("envelopeData", ex.ParamName);
		}
		[Test]
		public void Filter_EnvelopeDataIsEmtpy_OK()
		{
			var criteria = FixtureData.TestCriteria1();
			var envelopeDataList = new EnvelopeDataDTO[0].AsQueryable();

			_criteria.Filter(criteria.ConditionsJSON, 1, envelopeDataList);
		}
		[Test]
		public void Filter_OneCriteriaEqualOperation_ExpectedEmtpyFiltredEnvelope()
		{
			// We will filter data by property(field) 'Value", operation 'Equal' and its value 'test value'
			var criteria = GenerateCriteriaJSON("Value", ExpressionType.Equal, Guid.NewGuid().ToString());
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() { Name = "test 1", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 1" },
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 2"},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 3"},
			}.AsQueryable();


			var filtred = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable());

			Assert.AreEqual(0, filtred.Count(), "Expected 0 entries in filtred collection");
		}
		[Test]
		public void Filter_OneCriteriaEqualOperation_ExpectedOneFiltredEnvelope()
		{
			// We will filter data by property(field) 'Value", operation 'Equal' and its value 'test value'
			var criteria = GenerateCriteriaJSON("Value", ExpressionType.Equal, "test value 1");
			var expectedEnvelope = new EnvelopeDataDTO() { Name = "test 1", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 1" };
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				expectedEnvelope,
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 2"},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 3"},
			}.AsQueryable();


			var filtred = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable());
			var filtredEnvelope = filtred.FirstOrDefault();

			Assert.AreEqual(1, filtred.Count(), "Expected only 1 EnvelopeDataDTO");
			Assert.AreEqual(expectedEnvelope, filtredEnvelope, "Expected EnvelopeDataDTO[{0}]".format(expectedEnvelope.Name));

		}
		[Test]
		public void Filter_OneCriteriaEqualOperation_ExpectedTwoFiltredEnvelope()
		{
			// We will filter data by property(field) 'Value", operation 'Equal' and its value 'test value'
			var criteria = GenerateCriteriaJSON("Value", ExpressionType.Equal, "test value 1");
			var expectedEnvelope1 = new EnvelopeDataDTO() { Name = "test 1", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 1" };
			var expectedEnvelope2 = new EnvelopeDataDTO() { Name = "test 2", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 1" };
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				expectedEnvelope1,
				expectedEnvelope2,
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 3"},
			}.AsQueryable();

			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable()).ToList();

			Assert.AreEqual(2, filtredEnvelopes.Count(), "Expected only 2 EnvelopeDataDTO");
			Assert.Contains(expectedEnvelope1, filtredEnvelopes, "Expected EnvelopeDataDTO[{0}]".format(expectedEnvelope1.Name));
			Assert.Contains(expectedEnvelope2, filtredEnvelopes, "Expected EnvelopeDataDTO[{0}]".format(expectedEnvelope2.Name));
		}
		[Test]
		public void Filter_OneCriteriaEqualOperation_ExpectedNFiltredEnvelope()
		{
			// We will filter data by property(field) 'Value", operation 'Equal' and its value 'test value'
			var criteria = GenerateCriteriaJSON("Value", ExpressionType.Equal, "test value 1");
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 1", EnvelopeId = "1", RecipientId = "1", TabId = "0", Value = "test value 1"},
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "2", RecipientId = "2", TabId = "0", Value = "test value 1"},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "3", RecipientId = "3", TabId = "0", Value = "test value 1"},
				new EnvelopeDataDTO() {Name = "test 4", EnvelopeId = "4", RecipientId = "4", TabId = "0", Value = "test value 1"},
				new EnvelopeDataDTO() {Name = "test 5", EnvelopeId = "5", RecipientId = "5", TabId = "0", Value = "test value 1"},
			}.AsQueryable();

			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable()).ToList();
			HashSet<EnvelopeDataDTO> h1 = new HashSet<EnvelopeDataDTO>(envelopeDataList);
			HashSet<EnvelopeDataDTO> h2 = new HashSet<EnvelopeDataDTO>(filtredEnvelopes);
			h1.ExceptWith(h2);

			Assert.AreEqual(envelopeDataList.Count(), filtredEnvelopes.Count(), "Expected only {0} EnvelopeDataDTO".format(envelopeDataList.Count()));
			Assert.AreEqual(0, h1.Count, "Expeceted filterd EnvelopeDataDTO with all envelopeDataList");
		}
		[Test]
		public void Filter_OneCriteriaGreaterThanOperation_ExpectedEmtpyFiltredEnvelope()
		{
			// We will filter data by property(field) 'DocumentId", operation 'GreaterThan' and its value '4'
			var criteria = GenerateCriteriaJSON("DocumentId", ExpressionType.GreaterThan, "4");
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 1", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 1"},
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "2", RecipientId = "2", TabId = "0", Value = "test value 2"},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "3", RecipientId = "2", TabId = "0", Value = "test value 3"},
			}.AsQueryable();


			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable());

			Assert.AreEqual(0, filtredEnvelopes.Count(), "Expected 0 entries in filtred collection");
		}
		[Test]
		public void Filter_OneCriteriaGreaterThanOperation_ExpectedOneFiltredEnvelope()
		{
			// We will filter data by property(field) 'DocumentId", operation 'GreaterThan' and its value '4'
			var criteria = GenerateCriteriaJSON("DocumentId", ExpressionType.GreaterThan, "4");
			var expectedEnvelope = new EnvelopeDataDTO() { Name = "test 5", EnvelopeId = "5", RecipientId = "2", TabId = "0", Value = "test value 5", DocumentId = 5 };
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "2", RecipientId = "2", TabId = "0", Value = "test value 2", DocumentId = 2},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "3", RecipientId = "2", TabId = "0", Value = "test value 3", DocumentId = 3},
				new EnvelopeDataDTO() {Name = "test 4", EnvelopeId = "4", RecipientId = "2", TabId = "0", Value = "test value 4", DocumentId = 4},
				expectedEnvelope
			}.AsQueryable();


			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable());

			Assert.AreEqual(1, filtredEnvelopes.Count(), "Expected only 1 EnvelopeDataDTO");
			Assert.AreEqual(expectedEnvelope, filtredEnvelopes.FirstOrDefault(), "Expected EnvelopeDataDTO[{0}] with EnvelopeId: {1}".format(expectedEnvelope.Name, expectedEnvelope.EnvelopeId));
		}
		[Test]
		public void Filter_OneCriteriaGreaterThanOperation_ExpectedTwoFiltredEnvelope()
		{
			// We will filter data by property(field) 'DocumentId", operation 'GreaterThan' and its value '4'
			var criteria = GenerateCriteriaJSON("DocumentId", ExpressionType.GreaterThan, "4");
			var expectedEnvelope1 = new EnvelopeDataDTO() { Name = "test 5", EnvelopeId = "5", RecipientId = "2", TabId = "0", Value = "test value 5", DocumentId = 5 };
			var expectedEnvelope2 = new EnvelopeDataDTO() { Name = "test 6", EnvelopeId = "6", RecipientId = "2", TabId = "0", Value = "test value 6", DocumentId = 6 };
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 1", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 1", DocumentId = 1},
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "2", RecipientId = "2", TabId = "0", Value = "test value 2", DocumentId = 2},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "3", RecipientId = "2", TabId = "0", Value = "test value 3", DocumentId = 3},
				new EnvelopeDataDTO() {Name = "test 4", EnvelopeId = "4", RecipientId = "2", TabId = "0", Value = "test value 4", DocumentId = 4},
				expectedEnvelope1,
				expectedEnvelope2
			}.AsQueryable();


			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable()).ToList();

			Assert.AreEqual(2, filtredEnvelopes.Count(), "Expected only 2 EnvelopeDataDTO");
			Assert.Contains(expectedEnvelope1, filtredEnvelopes, "Expected EnvelopeDataDTO[{0}] with EnvelopeId: {1}".format(expectedEnvelope1.Name, expectedEnvelope1.EnvelopeId));
			Assert.Contains(expectedEnvelope2, filtredEnvelopes, "Expected EnvelopeDataDTO[{0}] with EnvelopeId: {1}".format(expectedEnvelope2.Name, expectedEnvelope2.EnvelopeId));
		}
		[Test]
		public void Filter_OneCriteriaGreaterThanOperation_ExpectedManyFiltredEnvelope()
		{
			// We will filter data by property(field) 'DocumentId", operation 'GreaterThan' and its value '0'
			var criteria = GenerateCriteriaJSON("DocumentId", ExpressionType.GreaterThan, "0");
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 1", EnvelopeId = "1", RecipientId = "1", TabId = "0", Value = "test value 1", DocumentId = 1},
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "2", RecipientId = "2", TabId = "0", Value = "test value 1", DocumentId = 2},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "3", RecipientId = "3", TabId = "0", Value = "test value 1", DocumentId = 3},
				new EnvelopeDataDTO() {Name = "test 4", EnvelopeId = "4", RecipientId = "4", TabId = "0", Value = "test value 1", DocumentId = 4},
				new EnvelopeDataDTO() {Name = "test 5", EnvelopeId = "5", RecipientId = "5", TabId = "0", Value = "test value 1", DocumentId = 5},
			}.AsQueryable();


			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable()).ToList();
			HashSet<EnvelopeDataDTO> h1 = new HashSet<EnvelopeDataDTO>(envelopeDataList);
			HashSet<EnvelopeDataDTO> h2 = new HashSet<EnvelopeDataDTO>(filtredEnvelopes);
			h1.ExceptWith(h2);

			Assert.AreEqual(envelopeDataList.Count(), filtredEnvelopes.Count(), "Expected only {0} EnvelopeDataDTO".format(envelopeDataList.Count()));
			Assert.AreEqual(0, h1.Count, "Expeceted filterd EnvelopeDataDTO with all envelopeDataList");
		}
		private string GenerateCriteriaJSON(string field, ExpressionType opType, string value)
		{
			const string template = @"{{""criteria"":[{{""field"":""{0}"",""operator"":""{1}"",""value"":""{2}""}}]}}";
			string @operator = opType == ExpressionType.Equal ? "Equals" : opType.ToString();

			return string.Format(template, field, @operator, value);
		}
	}
}
