using Core.Interfaces;
using Core.Services;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Moq;
using System.Collections.Generic;
using Utilities;
using Data.Interfaces.DataTransferObjects;
using Data.Entities;
using Data.States;

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
            var sampleEnvelope = FixtureData.TestEnvelope1();
            
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
        public void Execute_CriteriaEvaluateFalse_ReturnFalse()
        {
            //setup mock Criteria
            var mockCriteria = new Mock<ICriteria>();
            var envelopeDataList = FixtureData.TestEnvelopeDataList1();
            mockCriteria
                .Setup(c => c.Evaluate(It.IsAny<EnvelopeDO>(), It.IsAny<ProcessNodeDO>()))
                .Returns(false);
            ObjectFactory.Configure(cfg => cfg.For<ICriteria>().Use(mockCriteria.Object));
            _processNode = ObjectFactory.GetInstance<IProcessNode>();
            var processNodeDO = FixtureData.TestProcessNode();
            var envelopeDO = FixtureData.TestEnvelope1();

            string nextTransitionKey =_processNode.Execute(envelopeDO, processNodeDO);

            Assert.AreEqual("false", nextTransitionKey);
        }

        [Test]
        public void Execute_CriteriaEvaluateTrue_ReturnTrue()
        {
            //setup mock Criteria
            var mockCriteria = new Mock<ICriteria>();
            var envelopeDataList = FixtureData.TestEnvelopeDataList1();
            mockCriteria
                .Setup(c => c.Evaluate(It.IsAny<EnvelopeDO>(), It.IsAny<ProcessNodeDO>()))
                .Returns(true);
            ObjectFactory.Configure(cfg => cfg.For<ICriteria>().Use(mockCriteria.Object));
            //setup mock IActionList
            var actionListMock = new Mock<IActionList>();
            actionListMock.Setup(s => s.Process((ActionListDO)It.IsAny<object>())).Callback<ActionListDO>(p => { p.ActionListState = ActionListState.Completed; });
            ObjectFactory.Configure(cfg => cfg.For<IActionList>().Use(actionListMock.Object));
            _processNode = ObjectFactory.GetInstance<IProcessNode>();
            
            var processNodeDO = FixtureData.TestProcessNode2();
            var envelopeDO = FixtureData.TestEnvelope1();

            string nextTransitionKey = _processNode.Execute(envelopeDO, processNodeDO);

            Assert.AreEqual("true", nextTransitionKey);
        }

        [Test]
        public void Execute_ActionListTypeImmediate_CallProcess()
        {
            //setup mock Criteria
            var mockCriteria = new Mock<ICriteria>();
            var envelopeDataList = FixtureData.TestEnvelopeDataList1();
            mockCriteria
                .Setup(c => c.Evaluate(It.IsAny<EnvelopeDO>(), It.IsAny<ProcessNodeDO>()))
                .Returns(true);
            ObjectFactory.Configure(cfg => cfg.For<ICriteria>().Use(mockCriteria.Object));
            //setup mock IActionList
            var actionListMock = new Mock<IActionList>();
            actionListMock.Setup(s => s.Process((ActionListDO)It.IsAny<object>())).Callback<ActionListDO>(p => { p.ActionListState = ActionListState.Completed; });
            ObjectFactory.Configure(cfg => cfg.For<IActionList>().Use(actionListMock.Object));
            _processNode = ObjectFactory.GetInstance<IProcessNode>();

            var processNodeDO = FixtureData.TestProcessNode2();
            var envelopeDO = FixtureData.TestEnvelope1();

            string nextTransitionKey = _processNode.Execute(envelopeDO, processNodeDO);

            actionListMock.Verify(v => v.Process((ActionListDO)It.IsAny<object>()));
        }
	}
}