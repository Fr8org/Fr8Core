using System.Collections.Generic;
using Core.Interfaces;
using Core.Services;
using Data.Interfaces.DataTransferObjects;
using Moq;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Data.Entities;
using Data.States;
using Data.Interfaces;

namespace DockyardTest.Services
{
    [TestFixture]
	[Category("ProcessNode")]
	public class ProcessNodeTests : BaseTest
	{
		private IProcessNode _processNode;
		private IDocuSignNotification _docuSignNotificationService;
		private DockyardAccount _userService;
		private string _testUserId = "testuser";
		private string _xmlPayloadFullPath;

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			
			

		}

		[Test]
		public void ProcessNode_CanExecute()
		{
            //create a ProcessNode
            //add a mock for Criteria service that, when called with Evaluate method, returns true
            //verify that, when Execute is called, true is returned
            //SETUP
            var sampleNode = FixtureData.TestProcessNode();
		    var sampleEnvelope = FixtureData.TestEnvelopeDataList1();

            //setup mock Criteria
            var mockCriteria = new Mock<ICriteria>();
            string criteria = "fake criteria";
            int processId = 0;
            string envelopeId = "fake envelopeID";
            var envelopeDataList = FixtureData.TestEnvelopeDataList1();
            mockCriteria
                .Setup(c => c.Evaluate(It.IsAny<string>(), It.IsAny<int>(), (List<EnvelopeDataDTO>)It.IsAny<object>()))
                .Returns(true);
            ObjectFactory.Configure(cfg => cfg.For<ICriteria>().Use(mockCriteria.Object));
            _processNode = ObjectFactory.GetInstance<IProcessNode>();
            //EXECUTE
            _processNode.Execute(sampleEnvelope, sampleNode);

            //will throw exception if it fails
		}

        [Test]
        public void Execute_CriteriaEvaluatesToFalse_ReturnFalse()
        {
            //setup mock Criteria
            var mockCriteria = new Mock<ICriteria>();
            var envelopeDataList = FixtureData.TestEnvelopeDataList1();
            mockCriteria
                .Setup(c => c.Evaluate(It.IsAny<List<EnvelopeDataDTO>>(), It.IsAny<ProcessNodeDO>()))
                .Returns(false);
            ObjectFactory.Configure(cfg => cfg.For<ICriteria>().Use(mockCriteria.Object));
            _processNode = ObjectFactory.GetInstance<IProcessNode>();
            var processNodeDO = FixtureData.TestProcessNode();
            var docusignEventDO = FixtureData.TestEnvelopeDataList1();

            string nextTransitionKey = _processNode.Execute(docusignEventDO, processNodeDO);

            Assert.AreEqual("false", nextTransitionKey);
        }

        [Test]
        [Ignore("Requires update after v2 changes.")]
        public void Execute_CriteriaEvaluatesToTrue_ReturnTrue()
        {
            //setup mock Criteria
            var mockCriteria = new Mock<ICriteria>();
            var envelopeDataList = FixtureData.TestEnvelopeDataList1();
            mockCriteria
                .Setup(c => c.Evaluate(It.IsAny<List<EnvelopeDataDTO>>(), It.IsAny<ProcessNodeDO>()))
                .Returns(true);
            ObjectFactory.Configure(cfg => cfg.For<ICriteria>().Use(mockCriteria.Object));
            //setup mock IActionList
            var actionListMock = new Mock<IActionList>();
            actionListMock.Setup(s => s.Process((ActionListDO)It.IsAny<object>(), It.IsAny<ProcessDO>())).Callback<ActionListDO>(p => { p.ActionListState = ActionListState.Completed; });
            ObjectFactory.Configure(cfg => cfg.For<IActionList>().Use(actionListMock.Object));
            _processNode = ObjectFactory.GetInstance<IProcessNode>();

            var processNodeDO = FixtureData.TestProcessNode4();
            var docusignEventDO = FixtureData.TestEnvelopeDataList1();
            var processTemplate = FixtureData.TestProcessTemplate2();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ProcessTemplateRepository.Add(processTemplate);
                uow.ProcessNodeTemplateRepository.Add(processNodeDO.ProcessNodeTemplate);
                uow.SaveChanges();
            }

            string nextTransitionKey = _processNode.Execute(docusignEventDO, processNodeDO);

            Assert.AreEqual("true", nextTransitionKey);
        }

        [Test]
        [Ignore("Requires update after v2 changes.")]
        public void Execute_VerifyProcessCalled_ActionListTypeIsImmediate()
        {
            //setup mock Criteria
            var mockCriteria = new Mock<ICriteria>();
            var envelopeDataList = FixtureData.TestEnvelopeDataList1();
            mockCriteria
                .Setup(c => c.Evaluate(It.IsAny<List<EnvelopeDataDTO>>(), It.IsAny<ProcessNodeDO>()))
                .Returns(true);
            ObjectFactory.Configure(cfg => cfg.For<ICriteria>().Use(mockCriteria.Object));
            //setup mock IActionList
            var actionListMock = new Mock<IActionList>();
            actionListMock.Setup(s => s.Process((ActionListDO)It.IsAny<object>(), It.IsAny<ProcessDO>())).Callback<ActionListDO>(p => { p.ActionListState = ActionListState.Completed; });
            ObjectFactory.Configure(cfg => cfg.For<IActionList>().Use(actionListMock.Object));
            _processNode = ObjectFactory.GetInstance<IProcessNode>();

            var processNodeDO = FixtureData.TestProcessNode4();
            var docusignEventDO = FixtureData.TestEnvelopeDataList1();
            var processTemplate = FixtureData.TestProcessTemplate2();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ProcessTemplateRepository.Add(processTemplate);
                uow.ProcessNodeTemplateRepository.Add(processNodeDO.ProcessNodeTemplate);
                uow.SaveChanges();
            }

            string nextTransitionKey = _processNode.Execute(docusignEventDO, processNodeDO);

            actionListMock.Verify(v => v.Process((ActionListDO)It.IsAny<object>(), It.IsAny<ProcessDO>()));
        }
	}
}