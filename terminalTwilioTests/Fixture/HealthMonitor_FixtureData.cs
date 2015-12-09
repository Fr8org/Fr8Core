using System;
using Data.Interfaces.DataTransferObjects;

namespace terminalTwilioTests.Fixture
{
    class HealthMonitor_FixtureData
    {
        public static ActivityTemplateDTO Send_Via_Twilio_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = 1,
                Name = "Send_Via_Twilio_TEST",
                Version = "1"
            };
        }
        public static ActionDTO Send_Via_Twilio_v1_InitialConfiguration_ActionDTO()
        {
            var activityTemplate = Send_Via_Twilio_v1_ActivityTemplate();

            return new ActionDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Send_Via_Twilio",
                Label = "Send Via Twilio",
                AuthToken = Google_AuthToken1(),
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id
            };
        }
        public static AuthorizationTokenDTO Google_AuthToken1()
        {
            return new AuthorizationTokenDTO()
            {
                Token = @"{""AccessToken"":""ya29.OgLf-SvZTHJcdN9tIeNEjsuhIPR4b7KBoxNOuELd0T4qFYEa001kslf31Lme9OQCl6S5"",""RefreshToken"":""1/04H9hNCEo4vfX0nHHEdViZKz1CtesK8ByZ_TOikwVDc"",""Expires"":""2015-11-28T13:29:12.653075+05:00""}"
            };
        }
    }
}
