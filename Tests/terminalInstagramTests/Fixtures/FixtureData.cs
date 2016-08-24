using System;
using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Models;
using Fr8.Infrastructure.Data.Crates;

namespace terminalInstagramTests.Fixtures
{
    partial class FixtureData
    {
        public static ActivityTemplateSummaryDTO MonitorForNewMediaPosted_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Monitor_For_New_Media_Posted_TEST",
                Version = "1"
            };
        }

        public static Fr8DataDTO MonitorForNewMediaPosted_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = MonitorForNewMediaPosted_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Monitor For New Media Posts",
                ActivityTemplate = activityTemplate,
                AuthToken = Instagram_AuthToken()
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }


        public static AuthorizationTokenDTO Instagram_AuthToken()
        {
            return new AuthorizationTokenDTO
            {
                Token = @"test"
            };
        }

        public static ContainerExecutionContext InstagramContainerExecutionContext()
        {
            var instagramEventCM = new InstagramUserEventCM
            {
                MediaId = "123",
                UserId = "123",
                ChangedAspect = "media",
                Time = "123",
                SubscriptionId = "123"
            };
            var eventReportCrate = new EventReportCM
            {
                EventNames = "media",
                EventPayload = new CrateStorage(Crate.FromContent("Instagram user event", instagramEventCM))
            };
            var containerExecutionContext = new ContainerExecutionContext
            {
                PayloadStorage = new CrateStorage(Crate.FromContent(string.Empty, new OperationalStateCM()), Crate.FromContent("Instagram user event", eventReportCrate))
            };
            return containerExecutionContext;
        }

        public static ContainerExecutionContext FalseInstagramContainerExecutionContext()
        {
            var instagramEventCM = new InstagramUserEventCM
            {
                MediaId = "123",
                UserId = "123",
                ChangedAspect = "test",
                Time = "123",
                SubscriptionId = "123"
            };
            var eventReportCrate = new EventReportCM
            {
                EventNames = "media",
                EventPayload = new CrateStorage(Crate.FromContent("Instagram user event", instagramEventCM))
            };
            var containerExecutionContext = new ContainerExecutionContext
            {
                PayloadStorage = new CrateStorage(Crate.FromContent(string.Empty, new OperationalStateCM()), Crate.FromContent("Instagram user event", eventReportCrate))
            };
            return containerExecutionContext;
        }

    }
}
