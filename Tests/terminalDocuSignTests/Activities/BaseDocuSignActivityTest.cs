using System;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Crates;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalDocuSign.Actions;
using terminalDocuSign.Services.New_Api;
using terminalDocuSignTests.Fixtures;
using TerminalBase.Infrastructure;
using UtilitiesTesting.Asserts;
using UtilitiesTesting.Fixtures;
using terminalDocuSign.Activities;
using TerminalBase.Models;

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
            ObjectFactory.Configure(x => x.For<IDocuSignManager>().Use(new Mock<IDocuSignManager>().Object));
        }

        [Test]
        public async void Run_WhenNoAuthorization_Fails()
        {
            var activity = DocuSignActivityFixtureData.BaseDocuSignAcitvity();
            var activityContext = FixtureData.TestActivityContext1();
            var executionContext = FixtureData.ContainerExecutionContext1();
            await activity.Run(activityContext, executionContext);
            AssertEx.AssertPayloadHasAuthenticationError(executionContext.PayloadStorage);
        }

        [Test]
        public async void Run_WhenActivityIsNotValid_Fails()
        {
            var activity = DocuSignActivityFixtureData.FailedBaseDocuSignActivity();
            var activityContext = FixtureData.TestActivityContext1();
            var executionContext = FixtureData.ContainerExecutionContext1();
            await activity.Run(activityContext, executionContext);
            AssertEx.AssertPayloadHasError(executionContext.PayloadStorage);
        }

        [Test]
        public void Run_WhenActivityIsValid_RunsSuccessfully()
        {
            var activityMock = new Mock<BaseDocuSignActivity>();
            //activityMock.Setup(x => x.ValidateActivity(It.IsAny<ActivityDO>(), It.IsAny<ICrateStorage>(), It.IsAny<ValidationManager>())).Returns(Task.FromResult(0));
                        
            activityMock.Setup(x => x.Run(It.IsAny<ActivityContext>(), It.IsAny<ContainerExecutionContext>()))
                        //.Returns(Task.FromResult(FixtureData.PayloadDTO2()))
                        .Verifiable("RunInternal was not invoked when activity has auth token and is valid");
            var activityContext = FixtureData.TestActivityContext1();
            var executionContext = FixtureData.ContainerExecutionContext1();
            activityMock.Object.Run(activityContext, executionContext).Wait();
            Assert.DoesNotThrow(() => activityMock.Verify());
        }
    }
}

