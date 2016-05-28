using Data.Entities;
using Hub.Managers;
using Moq;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Constants;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using TerminalBase.Infrastructure;
using terminalFr8Core.Actions;
using terminalFr8Core.Activities;
using terminalTests.Fixtures;
using TerminalBase.Helpers;
using UtilitiesTesting;
using TerminalBase.Models;

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
            hubMock.Setup(x => x.GetActivityTemplates(It.IsAny<bool>()))
                   .Returns(Task.FromResult(ActivityTemplates));

            hubMock.Setup(x => x.GetActivityTemplates(It.IsAny<string>(), It.IsAny<bool>()))
                   .Returns<string, bool>((tags, getLatest) => Task.FromResult(ActivityTemplates.Where(x => x.Tags.Contains(tags)).ToList()));
            ObjectFactory.Container.Inject(hubMock);
            ObjectFactory.Container.Inject(hubMock.Object);
        }

        [Test]
        public async Task Configure_AfterInitialConfiguration_DataSourceSelectorContainsTableDataGenerators()
        {
            var activity = New<FilterObjectListByIncomingMessage_v1>();
            var activityContext = new ActivityContext
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage()

                },
                AuthorizationToken = new AuthorizationToken { Token = "1" }
            };
            //Initial config
            await activity.Configure(activityContext);
            Assert.AreEqual(1, activityContext.ActivityPayload.CrateStorage.GetReadonlyActivityUi<FilterObjectListByIncomingMessage_v1.ActivityUi>().DataSourceSelector.ListItems.Count, "Data source list is not properly populated");
        }

        [Test]
        public async Task Configure_WhenDataSourceIsChanged_RemoveAndReconfigureChildActivity()
        {
            var authToken = new AuthorizationToken { Token = "1" };
            var activity = New<FilterObjectListByIncomingMessage_v1>();
            var activityContext = new ActivityContext
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage(),
                    ChildrenActivities = new List<ActivityPayload>()
                },
                AuthorizationToken = authToken
            };
            //Initial config
            await activity.Configure(activityContext);
            activityContext.ActivityPayload.CrateStorage.UpdateControls<FilterObjectListByIncomingMessage_v1.ActivityUi>(x => x.DataSourceSelector.Value = ActivityTemplates[0].Id.ToString());
            await activity.Configure(activityContext);
            var hubMock = ObjectFactory.GetInstance<Mock<IHubCommunicator>>();

            hubMock.Verify(x => x.CreateAndConfigureActivity(ActivityTemplates[0].Id, It.IsAny<string>(), 1, activityContext.ActivityPayload.Id, It.IsAny<bool>(), It.IsAny<Guid?>()),
                           Times.Exactly(1),
                           "Child activity was not created and confgured");
        }

        [Test]
        public async Task Run_WhenNoDataIsCahced_RunsChildActivitiy()
        {
            var authToken = new AuthorizationToken { Token = "1" };
            var activity = New<FilterObjectListByIncomingMessage_v1>();
            var activityContext = new ActivityContext
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage(),
                    ChildrenActivities = new List<ActivityPayload>()
                },
                AuthorizationToken = authToken
            };
            var containerExecutionContext = HealthMonitor_FixtureData.ExecutionContextWithOnlyOperationalState();
            //Initial and followup config
            await activity.Configure(activityContext);
            activityContext.ActivityPayload.CrateStorage.UpdateControls<FilterObjectListByIncomingMessage_v1.ActivityUi>(x => x.DataSourceSelector.Value = ActivityTemplates[0].Id.ToString());
            await activity.Configure(activityContext);
            var childActivity = new ActivityPayload
            {
                ActivityTemplate = ActivityTemplates[0]
            };
            AddChild(activityContext.ActivityPayload, childActivity, 1);
            //Run
            await activity.Run(activityContext, containerExecutionContext);
            var operationalState = containerExecutionContext.PayloadStorage.FirstCrate<OperationalStateCM>().Content;
            Assert.AreEqual(ActivityResponse.Success.ToString(), operationalState.CurrentActivityResponse.Type, "Child activities should be ran during normal execution flow");
        }

        [Test]
        public async Task Run_Always_FiltersDataAndSkipsChildActivities()
        {
            //Setup
            var containerExecutionContext = HealthMonitor_FixtureData.ExecutionContextWithOnlyOperationalState();
            containerExecutionContext.PayloadStorage.Add(Crate<FieldDescriptionsCM>.FromContent("Message is here", new FieldDescriptionsCM(new FieldDTO("Message", "This message should be checked for keywords"))));
            var authToken = new AuthorizationToken { Token = "1" };
            var activity = New<FilterObjectListByIncomingMessage_v1>();
            var activityContext = new ActivityContext
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage(),
                    ChildrenActivities = new List<ActivityPayload>()
                },
                AuthorizationToken = authToken
            };
            //Initial and followup config
            await activity.Configure(activityContext);
            activityContext.ActivityPayload.CrateStorage.UpdateControls<FilterObjectListByIncomingMessage_v1.ActivityUi>(x =>
            {
                x.DataSourceSelector.Value = ActivityTemplates[0].Id.ToString();
                x.IncomingTextSelector.selectedKey = "Message";
            });
            await activity.Configure(activityContext);
            var crateStorage = activityContext.ActivityPayload.CrateStorage;
            var configurationValues = crateStorage.FirstCrate<FieldDescriptionsCM>(x => x.Label == "Configuration Values");
                configurationValues.Content.Fields.First(x => x.Key == "Cache Created At").Value = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            crateStorage.Add(Crate<StandardTableDataCM>.FromContent("Cached table",
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
            await activity.Configure(activityContext);
            var childActivity = new ActivityPayload
            {
                ActivityTemplate = ActivityTemplates[0]
            };
            AddChild(activityContext.ActivityPayload, childActivity,1) ;
            //Run
            await activity.Run(activityContext, containerExecutionContext);
            var operationalState = containerExecutionContext.PayloadStorage.FirstCrate<OperationalStateCM>().Content;
            Assert.AreEqual(ActivityResponse.SkipChildren.ToString(), operationalState.CurrentActivityResponse.Type, "Child activities should be skipped during normal execution flow");
            var filteredData = containerExecutionContext.PayloadStorage.FirstCrate<StandardTableDataCM>().Content;
            Assert.AreEqual(3, filteredData.Table.Count, "Filtered data doesn't contain proper row count");
        }
        private void AddChild(ActivityPayload parent, ActivityPayload child, int? ordering)
        {
            child.Ordering = ordering ?? (parent.ChildrenActivities.Count > 0 ? parent.ChildrenActivities.Max(x => x.Ordering) + 1 : 1);
            parent.ChildrenActivities.Add(child);
        }
    }
}
