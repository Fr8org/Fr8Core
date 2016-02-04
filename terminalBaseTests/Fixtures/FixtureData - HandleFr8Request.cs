using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace terminalTests.Fixtures
{
    public class Fixture_HandleRequest
    {
        public static ActivityTemplateDTO ActivityTemplateTest()
        {
            return new ActivityTemplateDTO()
            {
                Id = 1,
                Name = "terminalActionMock_TEST",
                Version = "1"
            };
        }

        public static ActivityTemplateDTO ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = 1,
                Name = "terminalActionMock",
                Version = "1"
            };
        }

        public static ActivityDTO terminalMockActionDTOTest()
        {
            var activityTemplate = ActivityTemplateTest();

            var activity = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Activity Mock",
                AuthToken = new AuthorizationTokenDTO(),
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id,
                ParentRouteNodeId = Guid.NewGuid(),
            };

            return activity;
        }

        public static ActivityDTO terminalMockActionDTO()
        {
            var activityTemplate = ActivityTemplate();

            var activity = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Activity Mock",
                AuthToken = new AuthorizationTokenDTO(),
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id,
                ParentRouteNodeId = Guid.NewGuid(),
            };

            return activity;
        }
    }
}
