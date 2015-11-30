using System;
using Data.Interfaces.DataTransferObjects;

namespace terminalDocuSignTests
{
    public class HealthMonitor_FixtureData
    {
        public static AuthorizationTokenDTO DocuSign_AuthToken()
        {
            return new AuthorizationTokenDTO()
            {
                Token = @"{ ""Email"": ""freight.testing@gmail.com"", ""ApiPassword"": ""SnByDvZJ/fp9Oesd/a9Z84VucjU="" }"
            };
        }

        public static ActivityTemplateDTO Monitor_DocuSign_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = 1,
                Name = "Monitor_DocuSign_TEST",
                Version = "1"
            };
        }

        public static ActivityTemplateDTO Receive_DocuSign_Envelope_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = 2,
                Name = "Receive_DocuSign_Envelope_TEST",
                Version = "1"
            };
        }

        public static ActionDTO Monitor_DocuSign_v1_InitialConfiguration_ActionDTO()
        {
            var activityTemplate = Monitor_DocuSign_v1_ActivityTemplate();

            return new ActionDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Monitor_DocuSign",
                Label = "Monitor DocuSign",
                AuthToken = DocuSign_AuthToken(),
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id
            };
        }

        public static ActionDTO Receive_DocuSign_Envelope_v1_Example_ActionDTO()
        {
            var activityTemplate = Receive_DocuSign_Envelope_v1_ActivityTemplate();

            return new ActionDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Receive_DocuSign",
                Label = "Receive DocuSign",
                AuthToken = DocuSign_AuthToken(),
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id
            };
        }

        public static ActionDTO Record_Docusign_v1_InitialConfiguration_ActionDTO()
        {
            var activityTemplate = Record_DocuSign_v1_ActivityTemplate();

            return new ActionDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Record_DocuSign",
                Label = "Record DocuSign",
                AuthToken = DocuSign_AuthToken(),
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id
            };
        }

        public static ActivityTemplateDTO Record_DocuSign_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = 3,
                Name = "Receive_DocuSign_Envelope_TEST",
                Version = "1"
            };
        }
    }
}
