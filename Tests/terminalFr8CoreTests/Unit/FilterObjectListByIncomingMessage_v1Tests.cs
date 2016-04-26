using Data.Constants;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using Moq;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TerminalBase.Infrastructure;
using terminalFr8Core.Actions;
using terminalTests.Fixtures;
using UtilitiesTesting;

namespace terminalTests.Integration
{
    [TestFixture]
    public class FilterObjectListByIncomingMessage_v1Tests : BaseTest
    {
        private static readonly List<ActivityTemplateDTO> ActivityTemplates = new List<ActivityTemplateDTO>
            {
                new ActivityTemplateDTO
                {
                    Id = Guid.NewGuid(),
                    Tags = Tags.TableDataGenerator,
                    Name = "Generator"
                },
                new ActivityTemplateDTO
                {
                    Id = Guid.NewGuid(),
                    Tags = string.Empty,
                    Name = "NotAGenerator"
                }
            };

        public override void SetUp()
        {
            base.SetUp();
            var hubMock = new Mock<IHubCommunicator>();
            //hubMock.Setup(x => x.DeleteExistingChildNodesFromActivity(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            hubMock.Setup(x => x.GetActivityTemplates(It.IsAny<string>()))
                   .Returns(Task.FromResult(ActivityTemplates));
            hubMock.Setup(x => x.GetActivityTemplates(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns<string, string>((tags, user) => Task.FromResult(ActivityTemplates.Where(x => x.Tags.Contains(tags)).ToList()));
            hubMock.Setup(x => x.GetPayload(It.IsAny<ActivityDO>(), It.IsAny<Guid>(), It.IsAny<string>()))
                   .Returns<ActivityDO, Guid, string>((act, contId, user) => Task.FromResult(HealthMonitor_FixtureData.PayloadWithOnlyOperationalState()));
            ObjectFactory.Container.Inject(hubMock);
            ObjectFactory.Container.Inject(hubMock.Object);
        }

        [Test]
        public async Task Configure_AfterInitialConfiguration_DataSourceSelectorContainsTableDataGenerators()
        {
            var activity = new FilterObjectListByIncomingMessage_v1();
            //Initial config
            var activityDO = await activity.Configure(new ActivityDO(), new AuthorizationTokenDO { Token = "1" });
            Assert.AreEqual(1, activityDO.GetReadonlyActivityUi<FilterObjectListByIncomingMessage_v1.ActivityUi>().DataSourceSelector.ListItems.Count, "Data source list is not properly populated");
        }

        [Test]
        public async Task Configure_WhenDataSourceIsChanged_RemoveAndReconfigureChildActivity()
        {
            var authToken = new AuthorizationTokenDO { Token = "1" };
            var activity = new FilterObjectListByIncomingMessage_v1();
            //Initial config
            var activityDO = await activity.Configure(new ActivityDO(), authToken);
            activityDO.UpdateControls<FilterObjectListByIncomingMessage_v1.ActivityUi>(x => x.DataSourceSelector.Value = ActivityTemplates[0].Id.ToString());
            activityDO = await activity.Configure(activityDO, authToken);
            var hubMock = ObjectFactory.GetInstance<Mock<IHubCommunicator>>();
            hubMock.Verify(x => x.DeleteExistingChildNodesFromActivity(activityDO.Id, It.IsAny<string>()),
                           Times.Exactly(1),
                           "Child activities were not deleted after data source was selected");
            hubMock.Verify(x => x.CreateAndConfigureActivity(ActivityTemplates[0].Id, It.IsAny<string>(), It.IsAny<string>(), 1, activityDO.Id, It.IsAny<bool>(), It.IsAny<Guid?>()),
                           Times.Exactly(1),
                           "Child activity was not created and confgured");
        }

        [Test]
        public async Task Run_WhenNoDataIsCahced_RunsChildActivitiy()
        {
            var authToken = new AuthorizationTokenDO { Token = "1" };
            var activity = new FilterObjectListByIncomingMessage_v1();
            //Initial and followup config
            var activityDO = await activity.Configure(new ActivityDO(), authToken);
            activityDO.UpdateControls<FilterObjectListByIncomingMessage_v1.ActivityUi>(x => x.DataSourceSelector.Value = ActivityTemplates[0].Id.ToString());
            activityDO = await activity.Configure(activityDO, authToken);
            activityDO.AddChild(new ActivityDO { ActivityTemplateId = ActivityTemplates[0].Id }, 1);
            //Run
            var result = await activity.Run(activityDO, Guid.NewGuid(), authToken);
            var operationalState = new CrateManager().GetStorage(result).FirstCrate<OperationalStateCM>().Content;
            Assert.AreEqual(ActivityResponse.Success.ToString(), operationalState.CurrentActivityResponse.Type, "Child activities should be ran during normal execution flow");
        }

        [Test]
        public async Task Run_Always_FiltersDataAndSkipsChildActivities()
        {
            //Setup
            var hubMock = ObjectFactory.GetInstance<Mock<IHubCommunicator>>();
            var payload = HealthMonitor_FixtureData.PayloadWithOnlyOperationalState();
            using (var storage = new CrateManager().GetUpdatableStorage(payload))
            {
                storage.Add(Crate<FieldDescriptionsCM>.FromContent("Message is here", new FieldDescriptionsCM(new FieldDTO("Message", "This message should be checked for keywords"))));
            }
            hubMock.Setup(x => x.GetPayload(It.IsAny<ActivityDO>(), It.IsAny<Guid>(), It.IsAny<string>()))
                    .Returns<ActivityDO, Guid, string>((act, contId, user) => Task.FromResult(payload));

            var authToken = new AuthorizationTokenDO { Token = "1" };
            var activity = new FilterObjectListByIncomingMessage_v1();
            //Initial and followup config
            var activityDO = await activity.Configure(new ActivityDO(), authToken);
            activityDO.UpdateControls<FilterObjectListByIncomingMessage_v1.ActivityUi>(x =>
            {
                x.DataSourceSelector.Value = ActivityTemplates[0].Id.ToString();
                x.IncomingTextSelector.selectedKey = "Message";
            });
            activityDO = await activity.Configure(activityDO, authToken);

            using (var storage = new CrateManager().GetUpdatableStorage(activityDO))
            {
                var configurationValues = storage.FirstCrate<FieldDescriptionsCM>(x => x.Label == "Configuration Values");
                configurationValues.Content.Fields.First(x => x.Key == "Cache Created At").Value = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                storage.Add(Crate<StandardTableDataCM>.FromContent("Cached table",
                    new StandardTableDataCM
                    {
                        FirstRowHeaders = true,
                        Table = new List<TableRowDTO>
                        {
                            new TableRowDTO { Row = new List<TableCellDTO> { new TableCellDTO { Cell = new FieldDTO("Header", "Header") } } },
                            //Will pass filter
                            new TableRowDTO { Row = new List<TableCellDTO> { new TableCellDTO { Cell = new FieldDTO("Header", "message") } } },
                            new TableRowDTO { Row = new List<TableCellDTO> { new TableCellDTO {  Cell = new FieldDTO("Header", "checked") } } },
                            //Won't pass filter
                            new TableRowDTO { Row = new List<TableCellDTO> { new TableCellDTO { Cell = new FieldDTO("Header", "nothing") } } },
                            new TableRowDTO { Row = new List<TableCellDTO> { new TableCellDTO { Cell = new FieldDTO("Header", "anything") } } }
                        }
                    }));
            }
            activityDO = await activity.Configure(activityDO, authToken);
            activityDO.AddChild(new ActivityDO { ActivityTemplateId = ActivityTemplates[0].Id }, 1);
            //Run
            var result = await activity.Run(activityDO, Guid.NewGuid(), authToken);
            var operationalState = new CrateManager().GetStorage(result).FirstCrate<OperationalStateCM>().Content;
            Assert.AreEqual(ActivityResponse.SkipChildren.ToString(), operationalState.CurrentActivityResponse.Type, "Child activities should be skipped during normal execution flow");
            var filteredData = new CrateManager().GetStorage(result).FirstCrate<StandardTableDataCM>().Content;
            Assert.AreEqual(3, filteredData.Table.Count, "Filtered data doesn't contain proper row count");
        }
    }
}
