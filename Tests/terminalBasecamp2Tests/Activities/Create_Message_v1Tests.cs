using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using Fr8.Testing.Unit;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using terminalBasecamp.Activities;
using terminalBasecamp.Infrastructure;

namespace terminalBasecamp2Tests.Activities
{
    [TestFixture]
    public class Create_Message_v1Tests : BaseTest
    {
        public override void SetUp()
        {
            base.SetUp();
            var basecampApiMock = new Mock<IBasecampApiClient>();
            ObjectFactory.Container.Inject(basecampApiMock);
            ObjectFactory.Container.Inject(basecampApiMock.Object);
        }

        private AuthorizationToken GetAuthorizationToken()
        {
            return new AuthorizationToken
            {
                Token = JsonConvert.SerializeObject(new BasecampAuthorizationToken())
            };
        }

        private ActivityContext GetActivityContext()
        {
            return new ActivityContext
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage()
                },
                AuthorizationToken = GetAuthorizationToken()
            };
        }

        [Test]
        public async Task Initialize_Always_LoadsListOfAccounts()
        {
            var activity = ObjectFactory.GetInstance<Create_Message_v1>();
            var context = GetActivityContext();
            await activity.Configure(context);
            ObjectFactory.GetInstance<Mock<IBasecampApiClient>>().Verify(
                                                                         x => x.GetAccounts(It.IsAny<AuthorizationToken>()),
                                                                         Times.Once(),
                                                                         "Initial configuration didn't load list of accounts");

        }
    }
}
