using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Constants;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalSalesforce.Actions;
using terminalSalesforce.Infrastructure;
using terminalSalesforceTests.Fixtures;
using TerminalBase.Infrastructure;
using UtilitiesTesting;
using TerminalBase.Models;
using Fr8Data.Managers;

namespace terminalSalesforceTests.Activities
{
    [TestFixture]
    [Category("terminalSalesforceTests")]
    public class Post_To_Chatter_v2Tests : BaseTest
    {
        private readonly AuthorizationTokenDO Token = new AuthorizationTokenDO { Token = "1" };

        public override void SetUp()
        {
            base.SetUp();
            TerminalBootstrapper.ConfigureTest();
            var salesforceManagerMock = new Mock<ISalesforceManager>();
            salesforceManagerMock.Setup(x => x.Query(SalesforceObjectType.Account, It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<AuthorizationToken>()))
                                 .Returns(Task.FromResult(new StandardTableDataCM
                                                          {
                                                              Table = new List<TableRowDTO>
                                                                      {
                                                                          new TableRowDTO { Row = new List<TableCellDTO> { new TableCellDTO { Cell = new FieldDTO("Id", "1") } } }
                                                                      }
                                                          }));
            salesforceManagerMock.Setup(x => x.PostToChatter(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AuthorizationToken>()))
                                 .Returns(Task.FromResult("feedid"));
            ObjectFactory.Container.Inject(salesforceManagerMock);
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
            var executionContext = new ContainerExecutionContext();
            var activityContext = new ActivityContext
            {
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage(Crate.FromContent(string.Empty, new OperationalStateCM()))
                },
                AuthorizationToken = {
                    Token = Token.Token
                }
            };
            await activity.Configure(activityContext);
            await activity.Run(activityContext, executionContext);
            var operationalState = new CrateManager().GetOperationalState(activityContext.ActivityPayload.CrateStorage);
            Assert.AreEqual(ActivityResponse.Error.ToString(), operationalState.CurrentActivityResponse.Type, "Run must fail if message is empty");
        }

        [Test]
        public async Task Run_WhenNoChatterSourceIsSelected_ReturnsError()
        {
            var activity = new Post_To_Chatter_v2();
            var executionContext = new ContainerExecutionContext();
            var activityContext = new ActivityContext
            {
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage(Crate.FromContent(string.Empty, new OperationalStateCM()))
                },
                AuthorizationToken = new AuthorizationToken {
                    Token = Token.Token
                }
            };
            await activity.Configure(activityContext);
            activityContext.ActivityPayload.CrateStorage.UpdateControls<Post_To_Chatter_v2.ActivityUi>(x =>
                                                                     {
                                                                         x.FeedTextSource.TextValue = "message";
                                                                         x.FeedTextSource.ValueSource = "specific";
                                                                     });
            await activity.Run(activityContext, executionContext);
            var operationalState = new CrateManager().GetOperationalState(activityContext.ActivityPayload.CrateStorage);
            Assert.AreEqual(ActivityResponse.Error.ToString(), operationalState.CurrentActivityResponse.Type, "Run must fail if chatter is not specified is empty");
        }

        [Test]
        public async Task Run_WhenQueriesSalesforceAndObjectsAreReturned_PostsToTheirChatters()
        {
            var activity = new Post_To_Chatter_v2();
            var executionContext = new ContainerExecutionContext();
            var activityContext = new ActivityContext
            {
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage(Crate.FromContent(string.Empty, new OperationalStateCM()))
                },
                AuthorizationToken = new AuthorizationToken {
                    Token = Token.Token
                }
            };
            await activity.Configure(activityContext);
            activityContext.ActivityPayload.CrateStorage.UpdateControls<Post_To_Chatter_v2.ActivityUi>(x =>
                                                                     {
                                                                         x.FeedTextSource.TextValue = "message";
                                                                         x.FeedTextSource.ValueSource = "specific";
                                                                         x.QueryForChatterOption.Selected = true;
                                                                         x.ChatterFilter.Value = string.Empty;
                                                                         x.ChatterSelector.selectedKey = SalesforceObjectType.Account.ToString();
                                                                     });
            await activity.Run(activityContext, executionContext);
            var payloadStorage = activityContext.ActivityPayload.CrateStorage;
            var operationalState = payloadStorage.FirstCrate<OperationalStateCM>().Content;
            Assert.AreEqual(ActivityResponse.Success.ToString(), operationalState.CurrentActivityResponse.Type, "Run must return success if all values are specified");
            var resultPayload = payloadStorage.FirstCrateOrDefault<StandardPayloadDataCM>();
            Assert.IsNotNull(resultPayload, "Successfull run should create standard payload data crate when Salesforce objects exist");
            Assert.AreEqual("feedid", resultPayload.Content.PayloadObjects[0].PayloadObject[0].Value, "Run didn't produce crate with proper posted feed Id");
        }

        [Test]
        public async Task Run_WhenQueriesSalesforceAndNothingIsReturned_ReturnsSuccessButNoPayload()
        {
            var activity = new Post_To_Chatter_v2();
            var executionContext = new ContainerExecutionContext();
            var activityContext = new ActivityContext
            {
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage(Crate.FromContent(string.Empty, new OperationalStateCM()))
                },
                AuthorizationToken = new AuthorizationToken {
                    Token = Token.Token
                }
            };
            await activity.Configure(activityContext);
            activityContext.ActivityPayload.CrateStorage.UpdateControls<Post_To_Chatter_v2.ActivityUi>(x =>
            {
                x.FeedTextSource.TextValue = "message";
                x.FeedTextSource.ValueSource = "specific";
                x.QueryForChatterOption.Selected = true;
                x.ChatterFilter.Value = string.Empty;
                x.ChatterSelector.selectedKey = SalesforceObjectType.Account.ToString();
            });
            ObjectFactory.GetInstance<Mock<ISalesforceManager>>().Setup(x => x.Query(It.IsAny<SalesforceObjectType>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<AuthorizationToken>()))
                         .Returns(Task.FromResult(new StandardTableDataCM { Table = new List<TableRowDTO>() }));
            await activity.Run(activityContext, executionContext);
            var payloadStorage = activityContext.ActivityPayload.CrateStorage;
            var operationalState = payloadStorage.FirstCrate<OperationalStateCM>().Content;
            Assert.AreEqual(ActivityResponse.Success.ToString(), operationalState.CurrentActivityResponse.Type, "Run must return success if all values are specified");
            var resultPayload = payloadStorage.FirstCrateOrDefault<StandardPayloadDataCM>();
            Assert.IsNull(resultPayload, "Successfull run should not create standard payload data crate when no Salesforce objects exist");
        }
    }
}