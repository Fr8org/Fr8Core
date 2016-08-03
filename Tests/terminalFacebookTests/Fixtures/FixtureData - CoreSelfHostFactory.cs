using System;
using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;

namespace terminalFacebookTests.Fixtures
{
    partial class FixtureData
    {
        public static ActivityTemplateSummaryDTO MonitorFr8Event_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Post_To_Timeline_TEST",
                Version = "1"
            };
        }

        public static Fr8DataDTO PostToTimeline_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = MonitorFr8Event_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Post To Timeline",
                ActivityTemplate = activityTemplate,
                AuthToken = Facebook_AuthToken()
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }

        public static ActivityTemplateSummaryDTO MonitorFeedPosts_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Monitor_Feed_Posts_TEST",
                Version = "1"
            };
        }

        public static Fr8DataDTO MonitorFeedPosts_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = MonitorFeedPosts_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Monitor Feed Posts",
                ActivityTemplate = activityTemplate,
                AuthToken = Facebook_AuthToken()
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }


        public static AuthorizationTokenDTO Facebook_AuthToken()
        {
            return new AuthorizationTokenDTO
            {
                Token = @"test"
            };
        }

    }
}
