using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Data.Entities;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalDocuSign.Actions;
using terminalDocuSign.Services.New_Api;
using terminalDocuSignTests.Fixtures;
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
            ObjectFactory.Configure(x => x.For<IDocuSignManager>().Use(new Mock<IDocuSignManager>().Object));
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
            activityMock.Setup(x => x.ValidateActivityInternal(It.IsAny<ActivityDO>()))
                        .Returns(ValidationResult.Success);
            activityMock.Setup(x => x.RunInternal(It.IsAny<ActivityDO>(), It.IsAny<Guid>(), It.IsNotNull<AuthorizationTokenDO>()))
                        .Returns(Task.FromResult(FixtureData.PayloadDTO2()))
                        .Verifiable("RunInternal was not invoked when activity has auth token and is valid");
            activityMock.Object.Run(FixtureData.TestActivity2(), Guid.Empty, FixtureData.TestActivityAuthenticate2()).Wait();
            Assert.DoesNotThrow(() => activityMock.Verify());
        }
    }
}

