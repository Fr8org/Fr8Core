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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

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

		/* One Criteria Equal Operation for string*/
		[Test]
		public void Filter_OneCriteriaEqualOpString_ExpectedEmtpyFiltredEnvelope()
		{
			// We will filter data by property(field) 'Value", operation 'Equal' and its value 'test value'
			var criteria = GenerateOneCriteriaJSON("Value", ExpressionType.Equal, Guid.NewGuid().ToString());
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
		public void Filter_OneCriteriaEqualOpString_ExpectedOneFiltredEnvelope()
		{
			// We will filter data by property(field) 'Value", operation 'Equal' and its value 'test value'
			var criteria = GenerateOneCriteriaJSON("Value", ExpressionType.Equal, "test value 1");
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
		public void Filter_OneCriteriaEqualOpString_ExpectedTwoFiltredEnvelope()
		{
			// We will filter data by property(field) 'Value", operation 'Equal' and its value 'test value'
			var criteria = GenerateOneCriteriaJSON("Value", ExpressionType.Equal, "test value 1");
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
		public void Filter_OneCriteriaEqualOpString_ExpectedNFiltredEnvelope()
		{
			// We will filter data by property(field) 'Value", operation 'Equal' and its value 'test value'
			var criteria = GenerateOneCriteriaJSON("Value", ExpressionType.Equal, "test value 1");
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 1", EnvelopeId = "1", RecipientId = "1", TabId = "0", Value = "test value 1"},
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "2", RecipientId = "2", TabId = "0", Value = "test value 1"},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "3", RecipientId = "3", TabId = "0", Value = "test value 1"},
				new EnvelopeDataDTO() {Name = "test 4", EnvelopeId = "4", RecipientId = "4", TabId = "0", Value = "test value 1"},
				new EnvelopeDataDTO() {Name = "test 5", EnvelopeId = "5", RecipientId = "5", TabId = "0", Value = "test value 1"},
			}.AsQueryable();

			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable()).ToList();
			HashSet<EnvelopeDataDTO> expectedHash = new HashSet<EnvelopeDataDTO>(envelopeDataList);
			HashSet<EnvelopeDataDTO> filtredHash = new HashSet<EnvelopeDataDTO>(filtredEnvelopes);
			var diff = expectedHash.Except(filtredHash);

			Assert.AreEqual(envelopeDataList.Count(), filtredEnvelopes.Count(), "Expected only {0} EnvelopeDataDTO".format(envelopeDataList.Count()));
			Assert.AreEqual(0, diff.Count(), "Expeceted filterd EnvelopeDataDTO with all envelopeDataList");
		}

		/* One Criteria Equal Operation for int*/
		[Test]
		public void Filter_OneCriteriaEqualOpInt_ExpectedEmtpyFiltredEnvelope()
		{
			// We will filter data by property(field) 'DocumentId", operation 'Equal' and its value '0'
			var criteria = GenerateOneCriteriaJSON("DocumentId", ExpressionType.Equal, "0");
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 1", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 1", DocumentId = 1 },
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 2", DocumentId = 2 },
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 3", DocumentId = 3 },
			}.AsQueryable();


			var filtred = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable());

			Assert.AreEqual(0, filtred.Count(), "Expected 0 entries in filtred collection");
		}
		[Test]
		public void Filter_OneCriteriaEqualOpInt_ExpectedOneFiltredEnvelope()
		{
			// We will filter data by property(field) 'DocumentId", operation 'Equal' and its value '1'
			var criteria = GenerateOneCriteriaJSON("DocumentId", ExpressionType.Equal, "1");
			var expectedEnvelope = new EnvelopeDataDTO() { Name = "test 1", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 1", DocumentId = 1 };
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				expectedEnvelope,
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 2", DocumentId = 2},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 3", DocumentId = 3},
			}.AsQueryable();


			var filtred = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable());
			var filtredEnvelope = filtred.FirstOrDefault();

			Assert.AreEqual(1, filtred.Count(), "Expected only 1 EnvelopeDataDTO");
			Assert.AreEqual(expectedEnvelope, filtredEnvelope, "Expected EnvelopeDataDTO[{0}]".format(expectedEnvelope.Name));

		}
		[Test]
		public void Filter_OneCriteriaEqualOpInt_ExpectedTwoFiltredEnvelope()
		{
			// We will filter data by property(field) 'DocumentId", operation 'Equal' and its value 'test value'
			var criteria = GenerateOneCriteriaJSON("DocumentId", ExpressionType.Equal, "1");
			var expectedEnvelope1 = new EnvelopeDataDTO() { Name = "test 1", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 1", DocumentId = 1 };
			var expectedEnvelope2 = new EnvelopeDataDTO() { Name = "test 2", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 1", DocumentId = 1 };
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				expectedEnvelope1,
				expectedEnvelope2,
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 3", DocumentId = 3},
			}.AsQueryable();

			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable()).ToList();

			Assert.AreEqual(2, filtredEnvelopes.Count(), "Expected only 2 EnvelopeDataDTO");
			Assert.Contains(expectedEnvelope1, filtredEnvelopes, "Expected EnvelopeDataDTO[{0}]".format(expectedEnvelope1.Name));
			Assert.Contains(expectedEnvelope2, filtredEnvelopes, "Expected EnvelopeDataDTO[{0}]".format(expectedEnvelope2.Name));
		}
		[Test]
		public void Filter_OneCriteriaEqualOpInt_ExpectedNFiltredEnvelope()
		{
			// We will filter data by property(field) 'DocumentId", operation 'Equal' and its value '1'
			var criteria = GenerateOneCriteriaJSON("DocumentId", ExpressionType.Equal, "1");
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 1", EnvelopeId = "1", RecipientId = "1", TabId = "0", Value = "test value 1", DocumentId = 1},
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "2", RecipientId = "2", TabId = "0", Value = "test value 1", DocumentId = 1},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "3", RecipientId = "3", TabId = "0", Value = "test value 1", DocumentId = 1},
				new EnvelopeDataDTO() {Name = "test 4", EnvelopeId = "4", RecipientId = "4", TabId = "0", Value = "test value 1", DocumentId = 1},
				new EnvelopeDataDTO() {Name = "test 5", EnvelopeId = "5", RecipientId = "5", TabId = "0", Value = "test value 1", DocumentId = 1},
			}.AsQueryable();

			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable()).ToList();
			HashSet<EnvelopeDataDTO> expectedHash = new HashSet<EnvelopeDataDTO>(envelopeDataList);
			HashSet<EnvelopeDataDTO> filtredHash = new HashSet<EnvelopeDataDTO>(filtredEnvelopes);
			var diff = expectedHash.Except(filtredHash);

			Assert.AreEqual(envelopeDataList.Count(), filtredEnvelopes.Count(), "Expected only {0} EnvelopeDataDTO".format(envelopeDataList.Count()));
			Assert.AreEqual(0, diff.Count(), "Expeceted filterd EnvelopeDataDTO with all envelopeDataList");
		}

		/* One Criteria GreaterThan Operation for int*/
		[Test]
		public void Filter_OneCriteriaGreaterThanOpInt_ExpectedEmtpyFiltredEnvelope()
		{
			// We will filter data by property(field) 'DocumentId", operation 'GreaterThan' and its value '4'
			var criteria = GenerateOneCriteriaJSON("DocumentId", ExpressionType.GreaterThan, "4");
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 1", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 1", DocumentId = 1},
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "2", RecipientId = "2", TabId = "0", Value = "test value 2", DocumentId = 2},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "3", RecipientId = "2", TabId = "0", Value = "test value 3", DocumentId = 3},
			}.AsQueryable();


			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable());

			Assert.AreEqual(0, filtredEnvelopes.Count(), "Expected 0 entries in filtred collection");
		}
		[Test]
		public void Filter_OneCriteriaGreaterThanOpInt_ExpectedOneFiltredEnvelope()
		{
			// We will filter data by property(field) 'DocumentId", operation 'GreaterThan' and its value '4'
			var criteria = GenerateOneCriteriaJSON("DocumentId", ExpressionType.GreaterThan, "4");
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
		public void Filter_OneCriteriaGreaterThanOpInt_ExpectedTwoFiltredEnvelope()
		{
			// We will filter data by property(field) 'DocumentId", operation 'GreaterThan' and its value '4'
			var criteria = GenerateOneCriteriaJSON("DocumentId", ExpressionType.GreaterThan, "4");
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
		public void Filter_OneCriteriaGreaterThanOpInt_ExpectedManyFiltredEnvelope()
		{
			// We will filter data by property(field) 'DocumentId", operation 'GreaterThan' and its value '0'
			var criteria = GenerateOneCriteriaJSON("DocumentId", ExpressionType.GreaterThan, "0");
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 1", EnvelopeId = "1", RecipientId = "1", TabId = "0", Value = "test value 1", DocumentId = 1},
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "2", RecipientId = "2", TabId = "0", Value = "test value 1", DocumentId = 2},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "3", RecipientId = "3", TabId = "0", Value = "test value 1", DocumentId = 3},
				new EnvelopeDataDTO() {Name = "test 4", EnvelopeId = "4", RecipientId = "4", TabId = "0", Value = "test value 1", DocumentId = 4},
				new EnvelopeDataDTO() {Name = "test 5", EnvelopeId = "5", RecipientId = "5", TabId = "0", Value = "test value 1", DocumentId = 5},
			}.AsQueryable();


			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable()).ToList();
			HashSet<EnvelopeDataDTO> expectedHash = new HashSet<EnvelopeDataDTO>(envelopeDataList);
			HashSet<EnvelopeDataDTO> filtredHash = new HashSet<EnvelopeDataDTO>(filtredEnvelopes);
			var diff = expectedHash.Except(filtredHash);

			Assert.AreEqual(envelopeDataList.Count(), filtredEnvelopes.Count(), "Expected only {0} EnvelopeDataDTO".format(envelopeDataList.Count()));
			Assert.AreEqual(0, diff.Count(), "Expeceted filterd EnvelopeDataDTO with all envelopeDataList");
		}

		/* One Criteria GreaterThanOrEqual Operation for int*/
		[Test]
		public void Filter_OneCriteriaGreaterThanOrEqualOpInt_ExpectedEmtpyFiltredEnvelope()
		{
			// We will filter data by property(field) 'DocumentId", operation 'GreaterThanOrEqual' and its value '4'
			var criteria = GenerateOneCriteriaJSON("DocumentId", ExpressionType.GreaterThanOrEqual, "4");
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 1", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 1", DocumentId = 1},
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "2", RecipientId = "2", TabId = "0", Value = "test value 2", DocumentId = 2 },
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "3", RecipientId = "2", TabId = "0", Value = "test value 3", DocumentId = 3},
			}.AsQueryable();


			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable());

			Assert.AreEqual(0, filtredEnvelopes.Count(), "Expected 0 entries in filtred collection");
		}
		[Test]
		public void Filter_OneCriteriaGreaterThanOrEqualOpInt_ExpectedOneFiltredEnvelope()
		{
			// We will filter data by property(field) 'DocumentId", operation 'GreaterThanOrEqual' and its value '4'
			var criteria = GenerateOneCriteriaJSON("DocumentId", ExpressionType.GreaterThanOrEqual, "4");
			var expectedEnvelope = new EnvelopeDataDTO() { Name = "test 5", EnvelopeId = "5", RecipientId = "2", TabId = "0", Value = "test value 5", DocumentId = 5 };
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "2", RecipientId = "2", TabId = "0", Value = "test value 2", DocumentId = 2},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "3", RecipientId = "2", TabId = "0", Value = "test value 3", DocumentId = 3},
				expectedEnvelope
			}.AsQueryable();


			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable());

			Assert.AreEqual(1, filtredEnvelopes.Count(), "Expected only 1 EnvelopeDataDTO");
			Assert.AreEqual(expectedEnvelope, filtredEnvelopes.FirstOrDefault(), "Expected EnvelopeDataDTO[{0}] with EnvelopeId: {1}".format(expectedEnvelope.Name, expectedEnvelope.EnvelopeId));
		}
		[Test]
		public void Filter_OneCriteriaGreaterThanOrEqualOpInt_ExpectedTwoFiltredEnvelope()
		{
			// We will filter data by property(field) 'DocumentId", operation 'GreaterThanOrEqual' and its value '4'
			var criteria = GenerateOneCriteriaJSON("DocumentId", ExpressionType.GreaterThanOrEqual, "4");
			var expectedEnvelope2 = new EnvelopeDataDTO() { Name = "test 4", EnvelopeId = "4", RecipientId = "2", TabId = "0", Value = "test value 4", DocumentId = 4 };
			var expectedEnvelope1 = new EnvelopeDataDTO() { Name = "test 5", EnvelopeId = "5", RecipientId = "2", TabId = "0", Value = "test value 5", DocumentId = 5 };
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 1", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 1", DocumentId = 1},
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "2", RecipientId = "2", TabId = "0", Value = "test value 2", DocumentId = 2},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "3", RecipientId = "2", TabId = "0", Value = "test value 3", DocumentId = 3},
				expectedEnvelope1,
				expectedEnvelope2
			}.AsQueryable();


			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable()).ToList();

			Assert.AreEqual(2, filtredEnvelopes.Count(), "Expected only 2 EnvelopeDataDTO");
			Assert.Contains(expectedEnvelope1, filtredEnvelopes, "Expected EnvelopeDataDTO[{0}] with EnvelopeId: {1}".format(expectedEnvelope1.Name, expectedEnvelope1.EnvelopeId));
			Assert.Contains(expectedEnvelope2, filtredEnvelopes, "Expected EnvelopeDataDTO[{0}] with EnvelopeId: {1}".format(expectedEnvelope2.Name, expectedEnvelope2.EnvelopeId));
		}
		[Test]
		public void Filter_OneCriteriaGreaterThanOrEqualOpInt_ExpectedManyFiltredEnvelope()
		{
			// We will filter data by property(field) 'DocumentId", operation 'GreaterThanOrEqual' and its value '1'
			var criteria = GenerateOneCriteriaJSON("DocumentId", ExpressionType.GreaterThanOrEqual, "1");
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 1", EnvelopeId = "1", RecipientId = "1", TabId = "0", Value = "test value 1", DocumentId = 1},
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "2", RecipientId = "2", TabId = "0", Value = "test value 1", DocumentId = 2},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "3", RecipientId = "3", TabId = "0", Value = "test value 1", DocumentId = 3},
				new EnvelopeDataDTO() {Name = "test 4", EnvelopeId = "4", RecipientId = "4", TabId = "0", Value = "test value 1", DocumentId = 4},
				new EnvelopeDataDTO() {Name = "test 5", EnvelopeId = "5", RecipientId = "5", TabId = "0", Value = "test value 1", DocumentId = 5},
			}.AsQueryable();


			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable()).ToList();
			HashSet<EnvelopeDataDTO> expectedHash = new HashSet<EnvelopeDataDTO>(envelopeDataList);
			HashSet<EnvelopeDataDTO> filtredHash = new HashSet<EnvelopeDataDTO>(filtredEnvelopes);
			var diff = expectedHash.Except(filtredHash);

			Assert.AreEqual(envelopeDataList.Count(), filtredEnvelopes.Count(), "Expected only {0} EnvelopeDataDTO".format(envelopeDataList.Count()));
			Assert.AreEqual(0, diff.Count(), "Expeceted filterd EnvelopeDataDTO with all envelopeDataList");
		}

		/* One Criteria LessThan Operation for int*/
		[Test]
		public void Filter_OneCriteriaLessThanOpInt_ExpectedEmtpyFiltredEnvelope()
		{
			// We will filter data by property(field) 'DocumentId", operation 'LessThan' and its value '1'
			var criteria = GenerateOneCriteriaJSON("DocumentId", ExpressionType.LessThan, "1");
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 1", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 1", DocumentId = 1},
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "2", RecipientId = "2", TabId = "0", Value = "test value 2", DocumentId = 2},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "3", RecipientId = "2", TabId = "0", Value = "test value 3", DocumentId = 3},
			}.AsQueryable();


			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable());

			Assert.AreEqual(0, filtredEnvelopes.Count(), "Expected 0 entries in filtred collection");
		}
		[Test]
		public void Filter_OneCriteriaLessThanOpInt_ExpectedOneFiltredEnvelope()
		{
			// We will filter data by property(field) 'DocumentId", operation 'LessThan' and its value '5'
			var criteria = GenerateOneCriteriaJSON("DocumentId", ExpressionType.LessThan, "5");
			var expectedEnvelope = new EnvelopeDataDTO() { Name = "test 4", EnvelopeId = "4", RecipientId = "2", TabId = "0", Value = "test value 4", DocumentId = 4 };
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 5", EnvelopeId = "5", RecipientId = "2", TabId = "0", Value = "test value 5", DocumentId = 5},
				new EnvelopeDataDTO() {Name = "test 6", EnvelopeId = "6", RecipientId = "2", TabId = "0", Value = "test value 6", DocumentId = 6},
				expectedEnvelope
			}.AsQueryable();


			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable());

			Assert.AreEqual(1, filtredEnvelopes.Count(), "Expected only 1 EnvelopeDataDTO");
			Assert.AreEqual(expectedEnvelope, filtredEnvelopes.FirstOrDefault(), "Expected EnvelopeDataDTO[{0}] with EnvelopeId: {1}".format(expectedEnvelope.Name, expectedEnvelope.EnvelopeId));
		}
		[Test]
		public void Filter_OneCriteriaLessThanOpInt_ExpectedTwoFiltredEnvelope()
		{
			// We will filter data by property(field) 'DocumentId", operation 'LessThan' and its value '6'
			var criteria = GenerateOneCriteriaJSON("DocumentId", ExpressionType.LessThan, "6");
			var expectedEnvelope2 = new EnvelopeDataDTO() { Name = "test 4", EnvelopeId = "4", RecipientId = "2", TabId = "0", Value = "test value 4", DocumentId = 4 };
			var expectedEnvelope1 = new EnvelopeDataDTO() { Name = "test 5", EnvelopeId = "5", RecipientId = "2", TabId = "0", Value = "test value 5", DocumentId = 5 };
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 6", EnvelopeId = "6", RecipientId = "2", TabId = "0", Value = "test value 6", DocumentId = 6},
				new EnvelopeDataDTO() {Name = "test 7", EnvelopeId = "7", RecipientId = "2", TabId = "0", Value = "test value 7", DocumentId = 7},
				expectedEnvelope1,
				expectedEnvelope2
			}.AsQueryable();


			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable()).ToList();

			Assert.AreEqual(2, filtredEnvelopes.Count(), "Expected only 2 EnvelopeDataDTO");
			Assert.Contains(expectedEnvelope1, filtredEnvelopes, "Expected EnvelopeDataDTO[{0}] with EnvelopeId: {1}".format(expectedEnvelope1.Name, expectedEnvelope1.EnvelopeId));
			Assert.Contains(expectedEnvelope2, filtredEnvelopes, "Expected EnvelopeDataDTO[{0}] with EnvelopeId: {1}".format(expectedEnvelope2.Name, expectedEnvelope2.EnvelopeId));
		}
		[Test]
		public void Filter_OneCriteriaLessThanOpInt_ExpectedManyFiltredEnvelope()
		{
			// We will filter data by property(field) 'DocumentId", operation 'LessThan' and its value '6'
			var criteria = GenerateOneCriteriaJSON("DocumentId", ExpressionType.LessThan, "6");
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 1", EnvelopeId = "1", RecipientId = "1", TabId = "0", Value = "test value 1", DocumentId = 1},
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "2", RecipientId = "2", TabId = "0", Value = "test value 1", DocumentId = 2},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "3", RecipientId = "3", TabId = "0", Value = "test value 1", DocumentId = 3},
				new EnvelopeDataDTO() {Name = "test 4", EnvelopeId = "4", RecipientId = "4", TabId = "0", Value = "test value 1", DocumentId = 4},
				new EnvelopeDataDTO() {Name = "test 5", EnvelopeId = "5", RecipientId = "5", TabId = "0", Value = "test value 1", DocumentId = 5},
			}.AsQueryable();


			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable()).ToList();
			HashSet<EnvelopeDataDTO> expectedHash = new HashSet<EnvelopeDataDTO>(envelopeDataList);
			HashSet<EnvelopeDataDTO> filtredHash = new HashSet<EnvelopeDataDTO>(filtredEnvelopes);
			expectedHash.ExceptWith(filtredHash);

			Assert.AreEqual(envelopeDataList.Count(), filtredEnvelopes.Count(), "Expected only {0} EnvelopeDataDTO".format(envelopeDataList.Count()));
			Assert.AreEqual(0, expectedHash.Count, "Expeceted filterd EnvelopeDataDTO with all envelopeDataList");
		}

		/* One Criteria LessThanOrEqual Operation for int*/
		[Test]
		public void Filter_OneCriteriaLessThanOrEqualOpInt_ExpectedEmtpyFiltredEnvelope()
		{
			// We will filter data by property(field) 'DocumentId", operation 'LessThanOrEqual' and its value '1'
			var criteria = GenerateOneCriteriaJSON("DocumentId", ExpressionType.LessThanOrEqual, "1");
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "2", RecipientId = "2", TabId = "0", Value = "test value 2", DocumentId = 2},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "3", RecipientId = "2", TabId = "0", Value = "test value 3", DocumentId = 3},
			}.AsQueryable();


			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable());

			Assert.AreEqual(0, filtredEnvelopes.Count(), "Expected 0 entries in filtred collection");
		}
		[Test]
		public void Filter_OneCriteriaLessThanOrEqualOpInt_ExpectedOneFiltredEnvelope()
		{
			// We will filter data by property(field) 'DocumentId", operation 'LessThanOrEqual' and its value '5'
			var criteria = GenerateOneCriteriaJSON("DocumentId", ExpressionType.LessThanOrEqual, "5");
			var expectedEnvelope = new EnvelopeDataDTO() { Name = "test 5", EnvelopeId = "5", RecipientId = "2", TabId = "0", Value = "test value 5", DocumentId = 5 };
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 6", EnvelopeId = "6", RecipientId = "2", TabId = "0", Value = "test value 6", DocumentId = 6},
				new EnvelopeDataDTO() {Name = "test 7", EnvelopeId = "7", RecipientId = "2", TabId = "0", Value = "test value 7", DocumentId = 7},
				expectedEnvelope
			}.AsQueryable();


			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable());

			Assert.AreEqual(1, filtredEnvelopes.Count(), "Expected only 1 EnvelopeDataDTO");
			Assert.AreEqual(expectedEnvelope, filtredEnvelopes.FirstOrDefault(), "Expected EnvelopeDataDTO[{0}] with EnvelopeId: {1}".format(expectedEnvelope.Name, expectedEnvelope.EnvelopeId));
		}
		[Test]
		public void Filter_OneCriteriaLessThanOrEqualOpInt_ExpectedTwoFiltredEnvelope()
		{
			// We will filter data by property(field) 'DocumentId", operation 'LessThanOrEqual' and its value '5'
			var criteria = GenerateOneCriteriaJSON("DocumentId", ExpressionType.LessThanOrEqual, "5");
			var expectedEnvelope2 = new EnvelopeDataDTO() { Name = "test 4", EnvelopeId = "4", RecipientId = "2", TabId = "0", Value = "test value 4", DocumentId = 4 };
			var expectedEnvelope1 = new EnvelopeDataDTO() { Name = "test 5", EnvelopeId = "5", RecipientId = "2", TabId = "0", Value = "test value 5", DocumentId = 5 };
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 6", EnvelopeId = "6", RecipientId = "2", TabId = "0", Value = "test value 6", DocumentId = 6},
				new EnvelopeDataDTO() {Name = "test 7", EnvelopeId = "7", RecipientId = "2", TabId = "0", Value = "test value 7", DocumentId = 7},
				expectedEnvelope1,
				expectedEnvelope2
			}.AsQueryable();


			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable()).ToList();

			Assert.AreEqual(2, filtredEnvelopes.Count(), "Expected only 2 EnvelopeDataDTO");
			Assert.Contains(expectedEnvelope1, filtredEnvelopes, "Expected EnvelopeDataDTO[{0}] with EnvelopeId: {1}".format(expectedEnvelope1.Name, expectedEnvelope1.EnvelopeId));
			Assert.Contains(expectedEnvelope2, filtredEnvelopes, "Expected EnvelopeDataDTO[{0}] with EnvelopeId: {1}".format(expectedEnvelope2.Name, expectedEnvelope2.EnvelopeId));
		}
		[Test]
		public void Filter_OneCriteriaLessThanOrEqualOpInt_ExpectedManyFiltredEnvelope()
		{
			// We will filter data by property(field) 'DocumentId", operation 'LessThanOrEqual' and its value '5'
			var criteria = GenerateOneCriteriaJSON("DocumentId", ExpressionType.LessThanOrEqual, "5");
			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 1", EnvelopeId = "1", RecipientId = "1", TabId = "0", Value = "test value 1", DocumentId = 1},
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "2", RecipientId = "2", TabId = "0", Value = "test value 1", DocumentId = 2},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "3", RecipientId = "3", TabId = "0", Value = "test value 1", DocumentId = 3},
				new EnvelopeDataDTO() {Name = "test 4", EnvelopeId = "4", RecipientId = "4", TabId = "0", Value = "test value 1", DocumentId = 4},
				new EnvelopeDataDTO() {Name = "test 5", EnvelopeId = "5", RecipientId = "5", TabId = "0", Value = "test value 1", DocumentId = 5},
			}.AsQueryable();


			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable()).ToList();
			HashSet<EnvelopeDataDTO> expectedHash = new HashSet<EnvelopeDataDTO>(envelopeDataList);
			HashSet<EnvelopeDataDTO> filtredHash = new HashSet<EnvelopeDataDTO>(filtredEnvelopes);
			var diff = expectedHash.Except(filtredHash);

			Assert.AreEqual(envelopeDataList.Count(), filtredEnvelopes.Count(), "Expected only {0} EnvelopeDataDTO".format(envelopeDataList.Count()));
			Assert.AreEqual(0, diff.Count(), "Expeceted filterd EnvelopeDataDTO with all envelopeDataList");
		}

		/* Two Criteria EqualOp And EqualOp String*/
		[Test]
		public void Filter_TwoCriteriaEqualOpAndEqualOpString_ExpectedEmptyFiltredEnvelope1()
		{
			// We will filter data by 2 criteria:
			// 1. property(field) 'Value", operation 'Equal' and its value 'Value'
			// 2. property(field) 'Value", operation 'Equal' and its value 'Value'
			var criteria = GenJSON(new List<Values>() 
			{
 				new Values("Value", ExpressionType.Equal, "Value"),
 				new Values("Value", ExpressionType.Equal, "Value"),
				
			});

			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 1", EnvelopeId = "1", RecipientId = "1", TabId = "0", Value = "test value 1", DocumentId = 1},
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "2", RecipientId = "2", TabId = "0", Value = "test value 1", DocumentId = 2},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "3", RecipientId = "3", TabId = "0", Value = "test value 1", DocumentId = 3},
				new EnvelopeDataDTO() {Name = "test 4", EnvelopeId = "4", RecipientId = "4", TabId = "0", Value = "test value 1", DocumentId = 4},
				new EnvelopeDataDTO() {Name = "test 5", EnvelopeId = "5", RecipientId = "5", TabId = "0", Value = "test value 1", DocumentId = 5},

			};
			var expected = envelopeDataList.Where(x => x.Value == "Value" && x.Value == "Value").ToList();

			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable()).ToList();

			HashSet<EnvelopeDataDTO> expectedHash = new HashSet<EnvelopeDataDTO>(expected);
			HashSet<EnvelopeDataDTO> filtredHash = new HashSet<EnvelopeDataDTO>(filtredEnvelopes);
			var diff = expectedHash.Except(filtredHash);
			Assert.AreEqual(0, diff.Count(), "Expected 0 entries in filtred collection");
		}
		[Test]
		public void Filter_TwoCriteriaEqualOpAndEqualOpString_ExpectedEmptyFiltredEnvelope2()
		{
			// We will filter data by 2 criteria(it will be always false):
			// 1. property(field) 'Value", operation 'Equal' and its value 'Value'
			// 2. property(field) 'Value", operation 'Equal' and its value 'Value1'
			var criteria = GenJSON(new List<Values>() 
			{
 				new Values("Value", ExpressionType.Equal, "Value"),
 				new Values("Value", ExpressionType.Equal, "Value1"),
				
			});

			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 1", EnvelopeId = "1", RecipientId = "1", TabId = "0", Value = "Value", DocumentId = 1},
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "2", RecipientId = "2", TabId = "0", Value = "Value1", DocumentId = 2},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "3", RecipientId = "3", TabId = "0", Value = "Value", DocumentId = 3},
				new EnvelopeDataDTO() {Name = "test 4", EnvelopeId = "4", RecipientId = "4", TabId = "0", Value = "Value1", DocumentId = 4},
				new EnvelopeDataDTO() {Name = "test 5", EnvelopeId = "5", RecipientId = "5", TabId = "0", Value = "Value", DocumentId = 5},

			};
			var expected = envelopeDataList.Where(x => x.Value == "Value" && x.Value == "Value1").ToList();

			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable()).ToList();

			HashSet<EnvelopeDataDTO> expectedHash = new HashSet<EnvelopeDataDTO>(expected);
			HashSet<EnvelopeDataDTO> filtredHash = new HashSet<EnvelopeDataDTO>(filtredEnvelopes);
			var diff = expectedHash.Except(filtredHash);
			Assert.AreEqual(0, diff.Count(), "Result by LINQ isn't equal to result by filter");
		}
		[Test]
		public void Filter_TwoCriteriaEqualOpAndEqualOpString_ExpectedTwoFiltredEnvelope()
		{
			// We will filter data by 2 criteria:
			// 1. property(field) 'Value", operation 'Equal' and its value 'Value'
			// 2. property(field) 'Value", operation 'Equal' and its value 'Value'
			var criteria = GenJSON(new List<Values>() 
			{
 				new Values("Value", ExpressionType.Equal, "Value"),
 				new Values("Value", ExpressionType.Equal, "Value"),
				
			});
			var expectedEnvelope1 = new EnvelopeDataDTO() { Name = "test 1", EnvelopeId = "1", RecipientId = "1", TabId = "0", Value = "Value", DocumentId = 1 };
			var expectedEnvelope2 = new EnvelopeDataDTO() { Name = "test 2", EnvelopeId = "2", RecipientId = "2", TabId = "0", Value = "Value", DocumentId = 2 };

			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				expectedEnvelope1,
				expectedEnvelope2,
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "3", RecipientId = "3", TabId = "0", Value = "test value 1", DocumentId = 3},
				new EnvelopeDataDTO() {Name = "test 4", EnvelopeId = "4", RecipientId = "4", TabId = "0", Value = "test value 1", DocumentId = 4},
				new EnvelopeDataDTO() {Name = "test 5", EnvelopeId = "5", RecipientId = "5", TabId = "0", Value = "test value 1", DocumentId = 5},

			};
			var expected = envelopeDataList.Where(x => x.Value == "Value" && x.Value == "Value").ToList();

			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable()).ToList();

			HashSet<EnvelopeDataDTO> expectedHash = new HashSet<EnvelopeDataDTO>(expected);
			HashSet<EnvelopeDataDTO> filtredHash = new HashSet<EnvelopeDataDTO>(filtredEnvelopes);
			var diff = expectedHash.Except(filtredHash);
			Assert.AreEqual(0, diff.Count(), "Result by LINQ isn't equal to result by filter");
			Assert.Contains(expectedEnvelope1, filtredEnvelopes, "Expected EnvelopeDataDTO[{0}] with EnvelopeId: {1}".format(expectedEnvelope1.Name, expectedEnvelope1.EnvelopeId));
			Assert.Contains(expectedEnvelope2, filtredEnvelopes, "Expected EnvelopeDataDTO[{0}] with EnvelopeId: {1}".format(expectedEnvelope2.Name, expectedEnvelope2.EnvelopeId));
		}

		/* Two Criteria EqualOp String And EqualOp Int*/
		[Test]
		public void Filter_TwoCriteriaEqualOpStringAndEqualOpInt_ExpectedEmptyFiltredEnvelope()
		{
			// We will filter data by 2 criteria:
			// 1. property(field) 'Value", operation 'Equal' and its value 'Value'
			// 2. property(field) 'DocumentId", operation 'Equal' and its value '1'
			var criteria = GenJSON(new List<Values>() 
			{
 				new Values("Value", ExpressionType.Equal, "Value"),
 				new Values("DocumentId", ExpressionType.Equal, "1"),
				
			});

			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 1", EnvelopeId = "1", RecipientId = "1", TabId = "0", Value = "Value1", DocumentId = 1},
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "2", RecipientId = "2", TabId = "0", Value = "Value", DocumentId = 2},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "3", RecipientId = "3", TabId = "0", Value = "test value 1", DocumentId = 3},
				new EnvelopeDataDTO() {Name = "test 4", EnvelopeId = "4", RecipientId = "4", TabId = "0", Value = "test value 1", DocumentId = 4},
				new EnvelopeDataDTO() {Name = "test 5", EnvelopeId = "5", RecipientId = "5", TabId = "0", Value = "test value 1", DocumentId = 5},

			};
			var expected = envelopeDataList.Where(x => x.Value == "Value" && x.DocumentId == 1).ToList();

			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable()).ToList();

			HashSet<EnvelopeDataDTO> expectedHash = new HashSet<EnvelopeDataDTO>(expected);
			HashSet<EnvelopeDataDTO> filtredHash = new HashSet<EnvelopeDataDTO>(filtredEnvelopes);
			var diff = expectedHash.Except(filtredHash);
			Assert.AreEqual(0, diff.Count(), "Result by LINQ isn't equal to result by filter");
		}
		[Test]
		public void Filter_TwoCriteriaEqualOpStringAndEqualOpInt_ExpectedOneFiltredEnvelope()
		{
			// We will filter data by 2 criteria:
			// 1. property(field) 'Value", operation 'Equal' and its value 'Value'
			// 2. property(field) 'DocumentId", operation 'Equal' and its value '1'
			var criteria = GenJSON(new List<Values>() 
			{
 				new Values("Value", ExpressionType.Equal, "Value"),
 				new Values("DocumentId", ExpressionType.Equal, "1"),
				
			});

			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 1", EnvelopeId = "1", RecipientId = "1", TabId = "0", Value = "Value", DocumentId = 1},
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "2", RecipientId = "2", TabId = "0", Value = "Value", DocumentId = 2},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "3", RecipientId = "3", TabId = "0", Value = "test value 1", DocumentId = 3},
				new EnvelopeDataDTO() {Name = "test 4", EnvelopeId = "4", RecipientId = "4", TabId = "0", Value = "test value 1", DocumentId = 4},
				new EnvelopeDataDTO() {Name = "test 5", EnvelopeId = "5", RecipientId = "5", TabId = "0", Value = "test value 1", DocumentId = 5},

			};
			var expected = envelopeDataList.Where(x => x.Value == "Value" && x.DocumentId == 1).ToList();

			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable()).ToList();

			HashSet<EnvelopeDataDTO> expectedHash = new HashSet<EnvelopeDataDTO>(expected);
			HashSet<EnvelopeDataDTO> filtredHash = new HashSet<EnvelopeDataDTO>(filtredEnvelopes);
			var diff = expectedHash.Except(filtredHash);
			Assert.AreEqual(0, diff.Count(), "Result by LINQ isn't equal to result by filter");
		}
		[Test]
		public void Filter_TwoCriteriaEqualOpStringAndEqualOpInt_ExpectedManyFiltredEnvelope()
		{
			// We will filter data by 2 criteria:
			// 1. property(field) 'Value", operation 'Equal' and its value 'Value'
			// 2. property(field) 'DocumentId", operation 'Equal' and its value '1'
			var criteria = GenJSON(new List<Values>() 
			{
 				new Values("Value", ExpressionType.Equal, "Value"),
 				new Values("DocumentId", ExpressionType.Equal, "1"),
				
			});

			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 1", EnvelopeId = "1", RecipientId = "1", TabId = "0", Value = "Value", DocumentId = 1},
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "2", RecipientId = "2", TabId = "0", Value = "Value", DocumentId = 1},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "3", RecipientId = "3", TabId = "0", Value = "Value", DocumentId = 1},
				new EnvelopeDataDTO() {Name = "test 4", EnvelopeId = "4", RecipientId = "4", TabId = "0", Value = "Value", DocumentId = 1},
				new EnvelopeDataDTO() {Name = "test 5", EnvelopeId = "5", RecipientId = "5", TabId = "0", Value = "Value", DocumentId = 1},

			};
			var expected = envelopeDataList.Where(x => x.Value == "Value" && x.DocumentId == 1).ToList();

			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable()).ToList();

			HashSet<EnvelopeDataDTO> expectedHash = new HashSet<EnvelopeDataDTO>(expected);
			HashSet<EnvelopeDataDTO> filtredHash = new HashSet<EnvelopeDataDTO>(filtredEnvelopes);
			var diff = expectedHash.Except(filtredHash);
			Assert.AreEqual(0, diff.Count(), "Result by LINQ isn't equal to result by filter");
		}

		/* Two Criteria GreaterThan Int And LessThanOrEquals Int*/
		[Test]
		public void Filter_TwoCriteriaGreaterThanOpIntAndLessThanOrEqualsOpInt_ExpectedOneFiltredEnvelope()
		{
			// We will filter data by 2 criteria:
			// 1. property(field) 'DocumentId", operation 'GreaterThan' and its value '1'
			// 2. property(field) 'DocumentId", operation 'LessThanOrEquals' and its value '6'
			// 1 < DocumentId <= 6
			var criteria = GenJSON(new List<Values>() 
			{
 				new Values("DocumentId", ExpressionType.GreaterThan, "1"),
 				new Values("DocumentId", ExpressionType.LessThanOrEqual, "6"),
				
			});

			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 0", EnvelopeId = "0", RecipientId = "0", TabId = "0", Value = "Value", DocumentId = 0},
				new EnvelopeDataDTO() {Name = "test 1", EnvelopeId = "1", RecipientId = "1", TabId = "0", Value = "Value", DocumentId = 1},
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "2", RecipientId = "2", TabId = "0", Value = "Value", DocumentId = 2},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "3", RecipientId = "3", TabId = "0", Value = "test value 1", DocumentId = 3},
				new EnvelopeDataDTO() {Name = "test 4", EnvelopeId = "4", RecipientId = "4", TabId = "0", Value = "test value 1", DocumentId = 4},
				new EnvelopeDataDTO() {Name = "test 5", EnvelopeId = "5", RecipientId = "5", TabId = "0", Value = "test value 1", DocumentId = 5},
				new EnvelopeDataDTO() {Name = "test 6", EnvelopeId = "6", RecipientId = "6", TabId = "0", Value = "test value 1", DocumentId = 6},
				new EnvelopeDataDTO() {Name = "test 7", EnvelopeId = "7", RecipientId = "7", TabId = "0", Value = "test value 1", DocumentId = 7},
				new EnvelopeDataDTO() {Name = "test 8", EnvelopeId = "8", RecipientId = "8", TabId = "0", Value = "test value 1", DocumentId = 8},

			};
			var expected = envelopeDataList.Where(x => x.DocumentId > 1 && x.DocumentId <= 6).ToList();

			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable()).ToList();

			HashSet<EnvelopeDataDTO> expectedHash = new HashSet<EnvelopeDataDTO>(expected);
			HashSet<EnvelopeDataDTO> filtredHash = new HashSet<EnvelopeDataDTO>(filtredEnvelopes);
			var diff = expectedHash.Except(filtredHash);
			Assert.AreEqual(0, diff.Count(), "Result by LINQ isn't equal to result by filter");
		}

		/* Three Criteria GreaterThan Int And LessThanOrEquals Int*/
		[Test]
		public void Filter_ThreeCriteriaGreaterThanOpIntAndLessThanOrEqualsOpIntAndEqualsOpString_ExpectedEmptyFiltredEnvelope()
		{
			// We will filter data by 2 criteria:
			// 1. property(field) 'DocumentId", operation 'GreaterThan' and its value '1'
			// 2. property(field) 'DocumentId", operation 'LessThanOrEquals' and its value '6'
			// 3. property(field) 'Value", operation 'Eqals' and its value 'Value'
			// 1 < DocumentId <= 6 and Value == "BBBBBBB"
			var criteria = GenJSON(new List<Values>() 
			{
 				new Values("DocumentId", ExpressionType.GreaterThan, "1"),
 				new Values("DocumentId", ExpressionType.LessThanOrEqual, "6"),
 				new Values("Value", ExpressionType.Equal,  "BBBBBBB"),
				
			});

			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 0", EnvelopeId = "0", RecipientId = "0", TabId = "0", Value = "Value", DocumentId = 0},
				new EnvelopeDataDTO() {Name = "test 1", EnvelopeId = "1", RecipientId = "1", TabId = "0", Value = "Value", DocumentId = 1},
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "2", RecipientId = "2", TabId = "0", Value = "Value", DocumentId = 2},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "3", RecipientId = "3", TabId = "0", Value = "test value 1", DocumentId = 3},
				new EnvelopeDataDTO() {Name = "test 4", EnvelopeId = "4", RecipientId = "4", TabId = "0", Value = "test value 1", DocumentId = 4},
				new EnvelopeDataDTO() {Name = "test 5", EnvelopeId = "5", RecipientId = "5", TabId = "0", Value = "Value", DocumentId = 5},
				new EnvelopeDataDTO() {Name = "test 6", EnvelopeId = "6", RecipientId = "6", TabId = "0", Value = "test value 1", DocumentId = 6},
				new EnvelopeDataDTO() {Name = "test 7", EnvelopeId = "7", RecipientId = "7", TabId = "0", Value = "test value 1", DocumentId = 7},
				new EnvelopeDataDTO() {Name = "test 8", EnvelopeId = "8", RecipientId = "8", TabId = "0", Value = "Value", DocumentId = 8},

			};
			var expected = envelopeDataList.Where(x => x.DocumentId > 1 && x.DocumentId <= 6 && x.Value == "BBBBBBB").ToList();

			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable()).ToList();

			HashSet<EnvelopeDataDTO> expectedHash = new HashSet<EnvelopeDataDTO>(expected);
			HashSet<EnvelopeDataDTO> filtredHash = new HashSet<EnvelopeDataDTO>(filtredEnvelopes);
			var diff = expectedHash.Except(filtredHash);
			Assert.AreEqual(0, diff.Count(), "Result by LINQ isn't equal to result by filter");
		}
		[Test]
		public void Filter_ThreeCriteriaGreaterThanOpIntAndLessThanOrEqualsOpIntAndEqualsOpString_ExpectedNFiltredEnvelope()
		{
			// We will filter data by 2 criteria:
			// 1. property(field) 'DocumentId", operation 'GreaterThan' and its value '1'
			// 2. property(field) 'DocumentId", operation 'LessThanOrEquals' and its value '6'
			// 3. property(field) 'Value", operation 'Eqals' and its value 'Value'
			// 1 < DocumentId <= 6 and Value == "Value"
			var criteria = GenJSON(new List<Values>() 
			{
 				new Values("DocumentId", ExpressionType.GreaterThan, "1"),
 				new Values("DocumentId", ExpressionType.LessThanOrEqual, "6"),
 				new Values("Value", ExpressionType.Equal, "Value"),
				
			});

			var envelopeDataList = new List<EnvelopeDataDTO>()
			{
				new EnvelopeDataDTO() {Name = "test 0", EnvelopeId = "0", RecipientId = "0", TabId = "0", Value = "Value", DocumentId = 0},
				new EnvelopeDataDTO() {Name = "test 1", EnvelopeId = "1", RecipientId = "1", TabId = "0", Value = "Value", DocumentId = 1},
				new EnvelopeDataDTO() {Name = "test 2", EnvelopeId = "2", RecipientId = "2", TabId = "0", Value = "Value", DocumentId = 2},
				new EnvelopeDataDTO() {Name = "test 3", EnvelopeId = "3", RecipientId = "3", TabId = "0", Value = "test value 1", DocumentId = 3},
				new EnvelopeDataDTO() {Name = "test 4", EnvelopeId = "4", RecipientId = "4", TabId = "0", Value = "test value 1", DocumentId = 4},
				new EnvelopeDataDTO() {Name = "test 5", EnvelopeId = "5", RecipientId = "5", TabId = "0", Value = "Value", DocumentId = 5},
				new EnvelopeDataDTO() {Name = "test 6", EnvelopeId = "6", RecipientId = "6", TabId = "0", Value = "test value 1", DocumentId = 6},
				new EnvelopeDataDTO() {Name = "test 7", EnvelopeId = "7", RecipientId = "7", TabId = "0", Value = "test value 1", DocumentId = 7},
				new EnvelopeDataDTO() {Name = "test 8", EnvelopeId = "8", RecipientId = "8", TabId = "0", Value = "Value", DocumentId = 8},

			};
			var expected = envelopeDataList.Where(x => x.DocumentId > 1 && x.DocumentId <= 6 && x.Value == "Value").ToList();

			var filtredEnvelopes = _criteria.Filter(criteria, 1, envelopeDataList.AsQueryable()).ToList();

			HashSet<EnvelopeDataDTO> expectedHash = new HashSet<EnvelopeDataDTO>(expected);
			HashSet<EnvelopeDataDTO> filtredHash = new HashSet<EnvelopeDataDTO>(filtredEnvelopes);
			var diff = expectedHash.Except(filtredHash);
			Assert.AreEqual(0, diff.Count(), "Result by LINQ isn't equal to result by filter");
		}
		private string GenJSON(List<Values> values)
		{
			TestCriteria t = new TestCriteria();
			foreach (var v in values)
				t.Criteria.Add(v);
			string res = JsonConvert.SerializeObject(t, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
			return res;
		}
		private class TestCriteria
		{
			public List<Values> Criteria;

			public TestCriteria()
			{
				Criteria = new List<Values>();
			}
		}
		private class Values
		{
			public string Field;
			public string Operator;
			public string Value;

			public Values(string field, ExpressionType opType, string value)
			{
				this.Field = field;
				this.Operator = GetOperator(opType);
				this.Value = value;
			}
		}
		private string GenerateOneCriteriaJSON(string field, ExpressionType opType, string value)
		{
			return GenJSON(new List<Values>() { new Values(field, opType, value) });
		}
		private static string GetOperator(ExpressionType opType)
		{
			string op = "";
			switch (opType)
			{
				case ExpressionType.Equal:
					op = "Equals";
					break;
				case ExpressionType.NotEqual:
					op = "NotEquals";
					break;
				case ExpressionType.GreaterThan:
					op = "GreaterThan";
					break;
				case ExpressionType.GreaterThanOrEqual:
					op = "GreaterThanOrEquals";
					break;
				case ExpressionType.LessThan:
					op = "LessThan";
					break;
				case ExpressionType.LessThanOrEqual:
					op = "LessThanOrEquals";
					break;

				default:
					break;
			}
			return op;
		}
	}
}
