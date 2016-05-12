using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Constants;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Hub.Managers;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalSalesforce.Actions;
using terminalSalesforce.Infrastructure;
using terminalSalesforceTests.Fixtures;
using TerminalBase.Infrastructure;
using UtilitiesTesting;

namespace terminalSalesforceTests.Activities
{
    [TestFixture, Ignore]
    [Category("terminalSalesforceTests")]
    public class Post_To_Chatter_v2Tests : BaseTest
    {
        private readonly AuthorizationTokenDO Token = new AuthorizationTokenDO { Token = "1" };

        public override void SetUp()
        {
            base.SetUp();
            TerminalBootstrapper.ConfigureTest();
            var salesforceManagerMock = new Mock<ISalesforceManager>();
            salesforceManagerMock.Setup(x => x.Query(SalesforceObjectType.Account, It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<AuthorizationTokenDO>()))
                                 .Returns(Task.FromResult(new StandardTableDataCM
                                                          {
                                                              Table = new List<TableRowDTO>
                                                                      {
                                                                          new TableRowDTO { Row = new List<TableCellDTO> { new TableCellDTO { Cell = new FieldDTO("Id", "1") } } }
                                                                      }
                                                          }));
            salesforceManagerMock.Setup(x => x.PostToChatter(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AuthorizationTokenDO>()))
                                 .Returns(Task.FromResult("feedid"));
            ObjectFactory.Container.Inject(salesforceManagerMock.Object);
            var hubCommunicatorMock = new Mock<IHubCommunicator>();
            ObjectFactory.Container.Inject(hubCommunicatorMock);
            ObjectFactory.Container.Inject(hubCommunicatorMock.Object);
            HealthMonitor_FixtureData.ConfigureHubToReturnEmptyPayload();
        }

        [Test]
        public async Task Run_WhenMessageIsEmpty_ReturnsError()
        {
            var activity = new Post_To_Chatter_v2();
            var activityDO = await activity.Configure(new ActivityDO(), Token);
            var result = await activity.Run(activityDO, Guid.Empty, Token);
            var operationalState = new CrateManager().GetOperationalState(result);
            Assert.AreEqual(ActivityResponse.Error.ToString(), operationalState.CurrentActivityResponse.Type, "Run must fail if message is empty");
        }

        [Test]
        public async Task Run_WhenNoChatterSourceIsSelected_ReturnsError()
        {
            var activity = new Post_To_Chatter_v2();
            var activityDO = await activity.Configure(new ActivityDO(), Token);
            activityDO.UpdateControls<Post_To_Chatter_v2.ActivityUi>(x =>
                                                                     {
                                                                         x.FeedTextSource.TextValue = "message";
                                                                         x.FeedTextSource.ValueSource = "specific";
                                                                     });
            var result = await activity.Run(activityDO, Guid.Empty, Token);
            var operationalState = new CrateManager().GetOperationalState(result);
            Assert.AreEqual(ActivityResponse.Error.ToString(), operationalState.CurrentActivityResponse.Type, "Run must fail if chatter is not specified is empty");
        }

        [Test]
        public async Task Run_WhenAllValuesAreSet_PostsToChatter()
        {
            var activity = new Post_To_Chatter_v2();
            var activityDO = await activity.Configure(new ActivityDO(), Token);
            activityDO.UpdateControls<Post_To_Chatter_v2.ActivityUi>(x =>
                                                                     {
                                                                         x.FeedTextSource.TextValue = "message";
                                                                         x.FeedTextSource.ValueSource = "specific";
                                                                         x.QueryForChatterOption.Selected = true;
                                                                         x.ChatterFilter.Value = string.Empty;
                                                                         x.ChatterSelector.selectedKey = SalesforceObjectType.Account.ToString();
                                                                     });
            var result = await activity.Run(activityDO, Guid.Empty, Token);
            var payloadStorage = new CrateManager().GetStorage(result);
            var operationalState = payloadStorage.FirstCrate<OperationalStateCM>().Content;
            Assert.AreEqual(ActivityResponse.Success.ToString(), operationalState.CurrentActivityResponse.Type, "Run must return success if all values are specified");
            var resultPayload = payloadStorage.FirstCrateOrDefault<StandardPayloadDataCM>();
            Assert.IsNotNull(resultPayload, "Successfull run should create standard payload data crate");
            Assert.AreEqual("feedid", resultPayload.Content.PayloadObjects[0].PayloadObject[0].Value, "Run didn't produce crate with proper posted feed Id");
        }
    }
}