using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace terminalSendGridTests.Fixtures
{
    public class HealthMonitor_FixtureData
    {
        public static ActivityTemplateDTO SendEmailViaSendGrid_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
                Name = "SendEmailViaSendGrid_TEST",
                Version = "1"
            };
        }

        public static Fr8DataDTO SendEmailViaSendGrid_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = SendEmailViaSendGrid_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Send Email",
                AuthToken = null,
                ActivityTemplate = activityTemplate
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }

    }
}
