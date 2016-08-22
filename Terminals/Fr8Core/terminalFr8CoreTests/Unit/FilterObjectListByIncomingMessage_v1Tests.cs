using Moq;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Helpers;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using terminalFr8Core.Activities;
using terminalTests.Fixtures;
using Fr8.Testing.Unit;

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

            hubMock.Setup(x => x.CreateAndConfigureActivity(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<Guid?>()))
                .Returns(() => Task.FromResult(new ActivityPayload() { ActivityTemplate = Mapper.Map<ActivityTemplateSummaryDTO>(ActivityTemplates[0]), Ordering = 1 }));
                
            ObjectFactory.Container.Inject(hubMock);
            ObjectFactory.Container.Inject(hubMock.Object);
        }

        [Test]
        public async Task Configure_AfterInitialConfiguration_DataSourceSelectorContainsTableDataGenerators()
        {
            var activity = New<Filter_Object_List_By_Incoming_Message_v1>();
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
            Assert.AreEqual(1, activityContext.ActivityPayload.CrateStorage.GetReadonlyActivityUi<Filter_Object_List_By_Incoming_Message_v1.ActivityUi>().DataSourceSelector.ListItems.Count, "Data source list is not properly populated");
        }

        [Test]
        public async Task Configure_WhenDataSourceIsChanged_RemoveAndReconfigureChildActivity()
        {
            var authToken = new AuthorizationToken { Token = "1" };
            var activity = New<Filter_Object_List_By_Incoming_Message_v1>();
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
            activityContext.ActivityPayload.CrateStorage.UpdateControls<Filter_Object_List_By_Incoming_Message_v1.ActivityUi>(x => x.DataSourceSelector.Value = ActivityTemplates[0].Id.ToString());
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
            var activity = New<Filter_Object_List_By_Incoming_Message_v1>();
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
            activityContext.ActivityPayload.CrateStorage.UpdateControls<Filter_Object_List_By_Incoming_Message_v1.ActivityUi>(x => x.DataSourceSelector.Value = ActivityTemplates[0].Id.ToString());
            await activity.Configure(activityContext);
            
            // var childActivity = new ActivityPayload
            // {
            //     ActivityTemplate = ActivityTemplates[0]
            // };
            // AddChild(activityContext.ActivityPayload, childActivity, 1);
            
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
            containerExecutionContext.PayloadStorage.Add(Crate<KeyValueListCM>.FromContent("Message is here", new KeyValueListCM(new KeyValueDTO("Message", "This message should be checked for keywords"))));
            var authToken = new AuthorizationToken { Token = "1" };
            var activity = New<Filter_Object_List_By_Incoming_Message_v1>();
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
            activityContext.ActivityPayload.CrateStorage.UpdateControls<Filter_Object_List_By_Incoming_Message_v1.ActivityUi>(x =>
            {
                x.DataSourceSelector.Value = ActivityTemplates[0].Id.ToString();
                x.IncomingTextSelector.selectedKey = "Message";
                x.IncomingTextSelector.Value = "This message should be checked for keywords";
            });
            await activity.Configure(activityContext);
            var crateStorage = activityContext.ActivityPayload.CrateStorage;
            var configurationValues = crateStorage.FirstCrate<KeyValueListCM>(x => x.Label == "Configuration Values");
                configurationValues.Content.Values.First(x => x.Key == "Cache Created At").Value = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            crateStorage.Add(Crate<StandardTableDataCM>.FromContent("Cached table",
                    new StandardTableDataCM
                    {
                        FirstRowHeaders = true,
                        Table = new List<TableRowDTO>
                        {
                            new TableRowDTO { Row = new List<TableCellDTO> { new TableCellDTO { Cell = new KeyValueDTO("Header", "Header") } } },
                            //Will pass filter
                            new TableRowDTO { Row = new List<TableCellDTO> { new TableCellDTO { Cell = new KeyValueDTO("Header", "message") } } },
                            new TableRowDTO { Row = new List<TableCellDTO> { new TableCellDTO {  Cell = new KeyValueDTO("Header", "checked") } } },
                            //Won't pass filter
                            new TableRowDTO { Row = new List<TableCellDTO> { new TableCellDTO { Cell = new KeyValueDTO("Header", "nothing") } } },
                            new TableRowDTO { Row = new List<TableCellDTO> { new TableCellDTO { Cell = new KeyValueDTO("Header", "anything") } } }
                        }
                    }));
            await activity.Configure(activityContext);
            var childActivity = new ActivityPayload
            {
                ActivityTemplate = Mapper.Map<ActivityTemplateSummaryDTO>(ActivityTemplates[0])
            };
            AddChild(activityContext.ActivityPayload, childActivity,1);
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
