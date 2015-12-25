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
                Name = "terminalBaseTests_TEST",
                Version = "1"
            };
        }

        public static ActivityTemplateDTO ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = 1,
                Name = "terminalBaseTests",
                Version = "1"
            };
        }

        public static ActionDTO terminalMockActionDTOTest()
        {
            var activityTemplate = ActivityTemplateTest();

            var action = new ActionDTO()
            {
                Id = Guid.NewGuid(),
                Name = "terminalActionMock",
                Label = "Action Mock",
                AuthToken = null,
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id,
                ParentRouteNodeId = Guid.NewGuid(),
            };

            return action;
        }

        public static ActionDTO terminalMockActionDTO()
        {
            var activityTemplate = ActivityTemplate();

            var action = new ActionDTO()
            {
                Id = Guid.NewGuid(),
                Name = "terminalActionMock",
                Label = "Action Mock",
                AuthToken = null,
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id,
                ParentRouteNodeId = Guid.NewGuid(),
            };

            return action;
        }
    }
}
