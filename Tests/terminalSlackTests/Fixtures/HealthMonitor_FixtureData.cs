using System;
using System.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.Interfaces;
using Moq;
using StructureMap;

namespace terminalSlackTests.Fixtures
{
    public class HealthMonitor_FixtureData
    {
        private static readonly CrateManager CrateManager = new CrateManager();

        public static Fr8DataDTO Monitor_Channel_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Monitor_Channel_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO
            {
                Id = Guid.NewGuid(),
                Label = "Monitor_Channel DocuSign",
                AuthToken = Slack_AuthToken(),
                ActivityTemplate = activityTemplate
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }

        public static ActivityTemplateSummaryDTO Monitor_Channel_v1_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO
            {
                Name = "Monitor_Channel_TEST",
                Version = "1"
            };
        }

        public static ActivityTemplateSummaryDTO Publish_To_Slack_v1_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO
            {
                Name = "Publish_To_Slack_TEST",
                Version = "1"
            };
        }

        public static AuthorizationTokenDTO Slack_AuthToken()
        {
            return new AuthorizationTokenDTO
            {
                Token = ConfigurationManager.AppSettings["SlackAuthToken"]
            };
        }


        internal static IEnumerable<KeyValueDTO> SlackEventFields()
        {
            return new List<KeyValueDTO>
            {
               new KeyValueDTO("token", "sU3N7wdnhXmml1zR2dVLf6PV"),
               new KeyValueDTO("team_id", "T07F83QLE"),
               new KeyValueDTO("team_domain", "dockyardteam"),
               new KeyValueDTO("service_id", "16193135954"),
               new KeyValueDTO("channel_id", "C07F83S86"),
               new KeyValueDTO("channel_name", "slack - plugin - test"),
               new KeyValueDTO("timestamp", "1449594901.000014"),
               new KeyValueDTO("user_id"," U0BNK9P1N"),
               new KeyValueDTO("user_name", "sergeyp"),
               new KeyValueDTO("text", "test")
           };
        }

        public static Fr8DataDTO Monitor_Channel_v1_InitialConfiguration_Fr8DataDTO(bool isAuthToken)
        {
            var activityTemplate = Monitor_Channel_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO
            {
                Id = Guid.NewGuid(),
                Label = "Monitor_Channel",
                AuthToken = isAuthToken ? Slack_AuthToken() : null,
                ActivityTemplate = activityTemplate
            };
            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }

        public static Fr8DataDTO Publish_To_Slack_v1_InitialConfiguration_Fr8DataDTO(bool isAuthToken = true)
        {
            var activityTemplate = Publish_To_Slack_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO
            {
                Id = Guid.NewGuid(),
                Label = "Selected_Slack_Channel",
                AuthToken = isAuthToken ? Slack_AuthToken() : null,
                ActivityTemplate = activityTemplate
            };
            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }

        public static void ConfigureHubToReturnEmptyPayload()
        {
            var result = new PayloadDTO(Guid.Empty);
            using (var storage = CrateManager.GetUpdatableStorage(result))
            {
                storage.Add(Crate.FromContent(string.Empty, new OperationalStateCM()));
            }
            ObjectFactory.Container.GetInstance<Mock<IHubCommunicator>>().Setup(x => x.GetPayload(It.IsAny<Guid>()))
                               .Returns(Task.FromResult(result));
        }
        public static ICrateStorage GetDirectMessageEventPayload()
        {
            var payload = new CrateStorage();
            payload.Add(Crate.FromContent(string.Empty, new OperationalStateCM()));
            var eventReport = new EventReportCM();
            eventReport.EventPayload.Add(Crate.FromContent(string.Empty, new StandardPayloadDataCM(new KeyValueDTO("channel_id", "D001"), new KeyValueDTO("user_name", "notuser"))));
            payload.Add(Crate.FromContent(string.Empty, eventReport));
            return payload;
        }

        public static void ConfigureHubToReturnPayloadWithChannelMessageEvent()
        {
            var result = new PayloadDTO(Guid.Empty);
            using (var storage = CrateManager.GetUpdatableStorage(result))
            {
                storage.Add(Crate.FromContent(string.Empty, new OperationalStateCM()));
                var eventReport = new EventReportCM();
                eventReport.EventPayload.Add(Crate.FromContent(string.Empty, new StandardPayloadDataCM(new KeyValueDTO("channel_id", "C001"), new KeyValueDTO("user_name", "notuser"))));
                storage.Add(Crate.FromContent(string.Empty, eventReport));
            }
            ObjectFactory.Container.GetInstance<Mock<IHubCommunicator>>().Setup(x => x.GetPayload(It.IsAny<Guid>()))
                               .Returns(Task.FromResult(result));
        }
    }
}
