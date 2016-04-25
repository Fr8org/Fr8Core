using System;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json;
using Hub.Managers;
using StructureMap;
using System.Collections.Generic;
using Data.Control;

namespace terminalAtlassianTests.Fixtures
{
    public class FixtureData
    {
        public static AuthorizationTokenDO JiraAuthorizationToken()
        {
            var curCredentialsDTO = new CredentialsDTO()
            {
                Domain = "https://maginot.atlassian.net",
                Username = "fr8_atlassian_test",
                Password = "yakima29"
            };

            return new AuthorizationTokenDO()
            {
                Token = JsonConvert.SerializeObject(curCredentialsDTO)
            };
        }

        public static ActivityDO GetJiraIssueTestActivityDO1()
        {
            ICrateManager _crate = ObjectFactory.GetInstance<ICrateManager>();

            var fieldEnterJiraKey = new TextSource()
            {
                Label = "Issue Number",
                Name = "IssueNumber",
                Required = true,
                Value = "FR-1245"
            };

            var fields = new List<ControlDefinitionDTO>()
            {
                fieldEnterJiraKey
            };

            var actionDo = new ActivityDO()
            {
                Id = Guid.NewGuid()
            };

            using (var crateStorage = _crate.GetUpdatableStorage(actionDo))
            {
                crateStorage.Add(Crate.FromContent("Configuration_Controls", new StandardConfigurationControlsCM(fields)));
            }
            return actionDo;
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

        public static PayloadDTO FakePayloadDTO
        {
            get
            {
                var payloadDTO = new PayloadDTO(TestContainerGuid());
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
