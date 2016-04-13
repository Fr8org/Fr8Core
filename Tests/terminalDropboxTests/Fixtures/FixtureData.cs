using System;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using Newtonsoft.Json;
using Ploeh.AutoFixture;

namespace terminalDropboxTests.Fixtures
{
    public class FixtureData
    {
        private static Fixture _fixture;

        static FixtureData()
        {
            // AutoFixture Setup
            _fixture = new Fixture();
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        public static AuthorizationTokenDO DropboxAuthorizationToken()
        {
            return _fixture.Build<AuthorizationTokenDO>()
                .With(x => x.Token, "bLgeJYcIkHAAAAAAAAAAFf6hjXX_RfwsFNTfu3z00zrH463seBYMNqBaFpbfBmqf")
                .OmitAutoProperties()
                .Create();
        }

        public static ActivityDO GetFileListActivityDO()
        {
            ActivityTemplateDO activityTemplateDO = _fixture.Build<ActivityTemplateDO>()
                 .With(x => x.Id)
                 .With(x => x.Name)
                 .With(x => x.Version)
                 .OmitAutoProperties()
                 .Create();
            ActivityDO activityDO = _fixture.Build<ActivityDO>()
                .With(x => x.Id)
                .With(x => x.ActivityTemplate, activityTemplateDO)
                .With(x => x.CrateStorage, string.Empty)
                .OmitAutoProperties()
                .Create();
            return activityDO;
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
            return new ActivityTemplateDO
            {
                Id = 1,
                Name = "Get File List",
                Version = "1"
            };
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
