using System;
using System.Threading.Tasks;
using Data.Entities;
using Moq;
using NUnit.Framework;
using StructureMap;
using TerminalBase.Infrastructure;
using UtilitiesTesting.Fixtures;

namespace terminalDocuSignTests.Actions
{
    public class BaseTest : UtilitiesTesting.BaseTest
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
    }
}
