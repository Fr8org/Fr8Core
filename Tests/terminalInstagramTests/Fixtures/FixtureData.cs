using System;
using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;

namespace terminalInstagramTests.Fixtures
{
    partial class FixtureData
    {
        public static ActivityTemplateDTO MonitorForNewMediaPosted_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
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

    }
}
