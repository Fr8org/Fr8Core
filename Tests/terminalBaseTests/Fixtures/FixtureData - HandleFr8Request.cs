using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace terminalTests.Fixtures
{
    public class Fixture_HandleRequest
    {
        public static ActivityTemplateDTO ActivityTemplateTest()
        {
            return new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
                Name = "terminalActivityMock_TEST",
                Version = "1"
            };
        }

        public static ActivityTemplateDTO ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
                Name = "BaseTerminalActivityMock",
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
