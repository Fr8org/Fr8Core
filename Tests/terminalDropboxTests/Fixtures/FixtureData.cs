using System;
using Data.Entities;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Hub.Managers;
using Ploeh.AutoFixture;
using TerminalBase.Models;
using Fr8Data.Managers;

namespace terminalDropboxTests.Fixtures
{
    public static class FixtureData
    {
        private static readonly Fixture Fixture;

        static FixtureData()
        {
            // AutoFixture Setup
            Fixture = new Fixture();
            Fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        public static AuthorizationToken DropboxAuthorizationToken()
        {
            return Fixture.Build<AuthorizationToken>()
                .With(x => x.Token, "bLgeJYcIkHAAAAAAAAAAFf6hjXX_RfwsFNTfu3z00zrH463seBYMNqBaFpbfBmqf")
                .OmitAutoProperties()
                .Create();
        }

        public static ActivityContext GetFileListActivityDO()
        {
            var terminalDTO = Fixture.Build<TerminalDTO>()
                 .With(x => x.Name)
                 .With(x => x.Version)
                 .OmitAutoProperties()
                 .Create();

            ActivityTemplateDTO activityTemplateDTO = Fixture.Build<ActivityTemplateDTO>()
                 .With(x => x.Id)
                 .With(x => x.Name)
                 .With(x => x.Version)
                 .With(x => x.Terminal, terminalDTO)
                 .OmitAutoProperties()
                 .Create();
            ActivityPayload activityPayload = Fixture.Build<ActivityPayload>()
                .With(x => x.Id)
                .With(x => x.ActivityTemplate, activityTemplateDTO)
                .With(x => x.CrateStorage, new CrateStorage())
                .OmitAutoProperties()
                .Create();
            ActivityContext activityContext = Fixture.Build<ActivityContext>()
                .With(x => x.ActivityPayload, activityPayload)
                .With(x => x.AuthorizationToken, DropboxAuthorizationToken())
                .OmitAutoProperties()
                .Create();
            return activityContext;
        }

        public static Guid TestContainerGuid()
        {
            return new Guid("70790811-3394-4B5B-9841-F26A7BE35163");
        }

        public static ContainerDO TestContainer()
        {
            var containerDO = new ContainerDO();
            containerDO.Id = TestContainerGuid();
            containerDO.State = 1;
            return containerDO;
        }

        public static ActivityTemplateDO GetFileListTestActivityTemplateDO()
        {
            ActivityTemplateDO activityTemplateDO = Fixture.Build<ActivityTemplateDO>()
                .With(x => x.Id)
                .With(x => x.Name, "Get File List")
                .With(x => x.Version, "1")
                .OmitAutoProperties()
                .Create();
            return activityTemplateDO;
        }

        public static PayloadDTO FakePayloadDTO
        {
            get
            {
                PayloadDTO payloadDTO = new PayloadDTO(TestContainerGuid());
                using (var crateStorage = new CrateManager().GetUpdatableStorage(payloadDTO))
                {
                    var operationalStatus = new OperationalStateCM();
                    var operationsCrate = Crate.FromContent("Operational Status", operationalStatus);
                    crateStorage.Add(operationsCrate);
                }
                return payloadDTO;
            }
        }
    }
}
