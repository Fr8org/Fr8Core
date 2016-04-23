using Data.Constants;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Hub.Managers;
using Moq;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerminalBase.Infrastructure;
using terminalFr8Core.Actions;
using UtilitiesTesting;

namespace terminalTests.Integration
{
    [TestFixture]
    public class FindObjectsThatMatchIncomingMessage_v1Tests : BaseTest
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
            ObjectFactory.Container.Inject(hubMock);
            ObjectFactory.Container.Inject(hubMock.Object);
        }

        [Test]
        public async Task Configure_AfterInitialConfiguration_DataSourceSelectorContainsTableDataGenerators()
        {
            var activity = new FindObjectsThatMatchIncomingMessage_v1();
            //Initial config
            var activityDO = await activity.Configure(new ActivityDO(), new AuthorizationTokenDO { Token = "1" });
            Assert.AreEqual(1, activityDO.GetReadonlyActivityUi<FindObjectsThatMatchIncomingMessage_v1.ActivityUi>().DataSourceSelector.ListItems.Count, "Data source list is not properly populated");
        }

        [Test]
        public async Task Configure_WhenDataSourceIsChanged_RemoveAndReconfigureChildActivity()
        {
            var authToken = new AuthorizationTokenDO { Token = "1" };
            var activity = new FindObjectsThatMatchIncomingMessage_v1();
            //Initial config
            var activityDO = await activity.Configure(new ActivityDO(), authToken);
            activityDO.UpdateControls<FindObjectsThatMatchIncomingMessage_v1.ActivityUi>(x => x.DataSourceSelector.Value = ActivityTemplates[0].Id.ToString());
            activityDO = await activity.Configure(activityDO, authToken);
            ObjectFactory.GetInstance<Mock<IHubCommunicator>>().Verify(
                x => x.DeleteExistingChildNodesFromActivity(activityDO.Id, It.IsAny<string>()),
                Times.Exactly(1),
                "Child activities were not deleted after data source was selected");
            
            //activityDO.UpdateControls<FindObjectsThatMatchIncomingMessage_v1.ActivityUi>(x => x.DataSourceSelector)
        }
    }
}
