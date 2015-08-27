using System.Collections.Generic;
using Core.Interfaces;
using Core.Services;
using Data.Interfaces.DataTransferObjects;
using Moq;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

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
			_processNode = ObjectFactory.GetInstance<IProcessNode>();
			

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

            //EXECUTE
            _processNode.Execute(sampleEnvelope, sampleNode);

            //will throw exception if it fails
		}


	}
}