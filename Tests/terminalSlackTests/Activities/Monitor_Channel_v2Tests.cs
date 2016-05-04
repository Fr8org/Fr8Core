using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8Data.DataTransferObjects;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalSlack.Interfaces;
using UtilitiesTesting;

namespace terminalSlackTests.Activities
{
    [TestFixture, Category("terminalSlack")]
    public class Monitor_Channel_v2Tests : BaseTest
    {
        public override void SetUp()
        {
            base.SetUp();
            var slackIntegrationMock = new Mock<ISlackIntegration>();
            slackIntegrationMock.Setup(x => x.GetChannelList(It.IsAny<string>(), It.IsAny<bool>()))
                                .Returns(Task.FromResult(new List<FieldDTO> { new FieldDTO("#channel", "1") }));
            ObjectFactory.Container.Inject(slackIntegrationMock.Object);
            var slackEventManagerMock = new Mock<ISlackEventManager>();
            ObjectFactory.Container.Inject(slackEventManagerMock.Object);
        }

        [Test]
        public void Initialize_Always_LoadsChannelList()
        {
            //var activity = new Monitor_Channel_v2();
            //var currentActivity = acti
        }
    }
}
