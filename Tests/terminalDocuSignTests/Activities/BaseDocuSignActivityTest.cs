using System;
using System.Threading.Tasks;
using Data.Entities;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalDocuSign.Actions;
using terminalDocuSignTests.Fixtures;
using TerminalBase.Infrastructure;
using UtilitiesTesting;
using UtilitiesTesting.Asserts;
using UtilitiesTesting.Fixtures;

namespace terminalDocuSignTests.Activities
{
    [TestFixture]
    [Category("BaseDocuSignActivity")]
    public class BaseDocuSignActivityTest : BaseTest
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            var hubCommunicatorMock = new Mock<IHubCommunicator>();

            hubCommunicatorMock.Setup(x => x.GetPayload(It.IsAny<ActivityDO>(), It.IsAny<Guid>(), It.IsAny<string>()))
                               .Returns(Task.FromResult(FixtureData.PayloadDTO2()));
            ObjectFactory.Configure(x => x.For<IHubCommunicator>().Use(hubCommunicatorMock.Object));
        }

        [Test]
        public void Run_WhenNoAuthorization_Fails()
        {
            var activity = DocuSignActivityFixtureData.BaseDocuSignAcitvity();
            var result = activity.Run(FixtureData.TestActivity2(), Guid.Empty, null).Result;
            AssertEx.AssertPayloadHasAuthenticationError(result);
        }

        [Test]
        public void Run_WhenActivityIsNotValid_Fails()
        {
            var activity = DocuSignActivityFixtureData.FailedBaseDocuSignActivity();
            var result = activity.Run(FixtureData.TestActivity2(), Guid.Empty, null).Result;
            AssertEx.AssertPayloadHasError(result);
        }

        [Test]
        public void Run_WhenActivityIsValid_RunsSuccessfully()
        {
            var activityMock = new Mock<BaseDocuSignActivity>();
            string errorMessage;
            activityMock.Setup(x => x.ActivityIsValid(It.IsAny<ActivityDO>(), out errorMessage))
                        .Returns(true);
            activityMock.Setup(x => x.RunInternal(It.IsAny<ActivityDO>(), It.IsAny<Guid>(), It.IsNotNull<AuthorizationTokenDO>()))
                        .Returns(Task.FromResult(FixtureData.PayloadDTO2()))
                        .Verifiable("RunInternal was not invoked when activity has auth token and is valid");
            activityMock.Object.Run(FixtureData.TestActivity2(), Guid.Empty, FixtureData.TestActivityAuthenticate2()).Wait();
            Assert.DoesNotThrow(() => activityMock.Verify());
        }
    }
}

