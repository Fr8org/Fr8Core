using Data.Entities;
using Moq;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerminalBase.Infrastructure;
using terminalSalesforce;
using terminalSalesforce.Actions;
using terminalSalesforce.Infrastructure;
using UtilitiesTesting;

namespace terminalSalesforceTests.Activities
{
    [TestFixture]
    [Category("terminalSalesforceTests")]
    public class Post_To_Chatter_v2Tests : BaseTest
    {
        public override void SetUp()
        {
            base.SetUp();
            TerminalBootstrapper.ConfigureTest();
            var salesforceManagerMock = new Mock<ISalesforceManager>();

        }

        [Test]
        public async Task Run_WhenMessageIsEmpty_ThrowsException()
        {
            //var activity = new Post_To_Chatter_v2();
            //var activityDO = await activity.Configure(new ActivityDO(), new AuthorizationTokenDO { Token = "1" });
        }
    }
}
