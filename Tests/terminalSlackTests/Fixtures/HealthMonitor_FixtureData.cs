using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using fr8.Infrastructure.Data.Crates;
using fr8.Infrastructure.Data.DataTransferObjects;
using fr8.Infrastructure.Data.Managers;
using fr8.Infrastructure.Data.Manifests;
using fr8.Infrastructure.Data.States;
using Moq;
using StructureMap;
using TerminalBase.Infrastructure;

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

        public static ActivityTemplateDTO Monitor_Channel_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO
            {
                Id = Guid.NewGuid(),
                Name = "Monitor_Channel_TEST",
                Label = "Monitor Channel",
                Category = ActivityCategory.Monitors,
                Terminal = new TerminalDTO
                {
                    AuthenticationType = AuthenticationType.Internal
                },
                Version = "1"
            };
        }

        public static ActivityTemplateDTO Publish_To_Slack_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO
            {
                Id = Guid.NewGuid(),
                Name = "Publish_To_Slack_TEST",
                Label = "Publish To Slack",
                Category = ActivityCategory.Forwarders,
                Version = "1",
                Terminal = new TerminalDTO
                {
                    AuthenticationType = AuthenticationType.Internal
                }
            };
        }

        public static AuthorizationTokenDTO Slack_AuthToken()
        {
            return new AuthorizationTokenDTO
            {
                Token = @"xoxp-9815816992-9816213634-14997343526-d99a1c9198"
            };
        }


        internal static IEnumerable<FieldDTO> SlackEventFields()
        {
            return new List<FieldDTO>
            {
               new FieldDTO("token", "sU3N7wdnhXmml1zR2dVLf6PV"),
               new FieldDTO("team_id", "T07F83QLE"),
               new FieldDTO("team_domain", "dockyardteam"),
               new FieldDTO("service_id", "16193135954"),
               new FieldDTO("channel_id", "C09Q069KL"),
               new FieldDTO("channel_name", "slack - plugin - test"),
               new FieldDTO("timestamp", "1449594901.000014"),
               new FieldDTO("user_id"," U0BNK9P1N"),
               new FieldDTO("user_name", "sergeyp"),
               new FieldDTO("text", "test")
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
            eventReport.EventPayload.Add(Crate.FromContent(string.Empty, new StandardPayloadDataCM(new FieldDTO("channel_id", "D001"), new FieldDTO("user_name", "notuser"))));
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
                eventReport.EventPayload.Add(Crate.FromContent(string.Empty, new StandardPayloadDataCM(new FieldDTO("channel_id", "C001"), new FieldDTO("user_name", "notuser"))));
                storage.Add(Crate.FromContent(string.Empty, eventReport));
            }
            ObjectFactory.Container.GetInstance<Mock<IHubCommunicator>>().Setup(x => x.GetPayload(It.IsAny<Guid>()))
                               .Returns(Task.FromResult(result));
        }
    }
}
