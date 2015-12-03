using System;
using Data.Interfaces.DataTransferObjects;

namespace terminalGoogleTests.Unit
{
    public class HealthMonitor_FixtureData
    {
        public static AuthorizationTokenDTO Google_AuthToken()
        {
            return new AuthorizationTokenDTO()
            {
                Token = @"{""AccessToken"":""ya29.PwJez2aHwjGxsxcho6TfaFseWjPbi1ThgINsgiawOKLlzyIgFJHkRdq76YrnuiGT3jhr"",""RefreshToken"":""1/HVhoZXzxFrPyC0JVlbEIF_VOBDm_IhrKoLKnt6QpyFRIgOrJDtdun6zK6XiATCKT"",""Expires"":""2015-12-03T11:12:43.0496208+08:00""}";
            };
        }


        public static ActivityTemplateDTO Receive_Google_Form_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = 1,
                Name = "Receive_Google_Form_TEST",
                Version = "1"
            };
        }

        public static ActionDTO Receive_Google_Form_v1_InitialConfiguration_ActionDTO()
        {
            var activityTemplate = Receive_Google_Form_v1_ActivityTemplate();

            return new ActionDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Receive_Google_Form",
                Label = "Receive Google Form Response",
                AuthToken = Google_AuthToken(),
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id
            };
        }
    }
}
