using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace terminalTests.Fixtures
{
    public class Fixture_HandleRequest
    {
        public static ActivityTemplateSummaryDTO ActivityTemplateTest()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "terminalActivityMock_TEST",
                Version = "1"
            };
        }

        public static ActivityTemplateSummaryDTO ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "ExplicitTerminalActivityMock",
                Version = "1"
            };
        }

        public static ActivityDTO terminalMockActivityDTOTest()
        {
            var activityTemplate = ActivityTemplateTest();

            var activity = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Activity Mock",
                AuthToken = new AuthorizationTokenDTO(),
                ActivityTemplate = activityTemplate,
                ParentPlanNodeId = Guid.NewGuid()
            };

            return activity;
        }

        public static Fr8DataDTO terminalMockFr8DataDTO()
        {
            return new Fr8DataDTO
            {
                ActivityDTO = terminalMockActivityDTO()
            };
        }

        public static ActivityDTO terminalMockActivityDTO()
        {
            var activityTemplate = ActivityTemplate();

            var activity = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Activity Mock",
                AuthToken = new AuthorizationTokenDTO(),
                ActivityTemplate = activityTemplate,
                ParentPlanNodeId = Guid.NewGuid()
            };

            return activity;
        }
    }
}
