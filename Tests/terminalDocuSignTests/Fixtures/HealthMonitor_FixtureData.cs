using System;
using Data.Interfaces.DataTransferObjects;

namespace terminalDocuSignTests.Fixtures
{
    public class HealthMonitor_FixtureData
    {
        public static AuthorizationTokenDTO DocuSign_AuthToken()
        {
            return new AuthorizationTokenDTO()
            {
                UserId = "testUser",
                Token = @"{ ""Email"": ""freight.testing@gmail.com"", ""ApiPassword"": ""SnByDvZJ/fp9Oesd/a9Z84VucjU="" }"
            };
        }

        public static ActivityTemplateDTO Monitor_DocuSign_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = 1,
                Name = "Monitor_DocuSign_Envelope_Activity_TEST",
                Version = "1"
            };
        }

        public static ActivityTemplateDTO Query_DocuSign_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = 1,
                Name = "Query_DocuSign_TEST",
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

        public static ActivityTemplateDTO Send_DocuSign_Envelope_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = 3,
                Name = "Send_DocuSign_Envelope_TEST",
                Version = "1"
            };
        }

        public static ActivityDTO Monitor_DocuSign_v1_InitialConfiguration_ActionDTO()
        {
            var activityTemplate = Monitor_DocuSign_v1_ActivityTemplate();

            return new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Monitor DocuSign Envelope Activity",
                AuthToken = DocuSign_AuthToken(),
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id
            };
        }

        public static ActivityDTO Query_DocuSign_v1_InitialConfiguration_ActionDTO()
        {
            var activityTemplate = Query_DocuSign_v1_ActivityTemplate();

            return new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Query DocuSign",
                AuthToken = DocuSign_AuthToken(),
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id
            };
        }

        public static ActivityDTO Receive_DocuSign_Envelope_v1_Example_ActionDTO()
        {
            var activityTemplate = Receive_DocuSign_Envelope_v1_ActivityTemplate();

            return new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Receive DocuSign",
                AuthToken = DocuSign_AuthToken(),
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id
            };
        }

        public static ActivityDTO Record_Docusign_v1_InitialConfiguration_ActionDTO()
        {
            var activityTemplate = Record_DocuSign_Envelope_v1_ActivityTemplate();

            return new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Record DocuSign",
                AuthToken = DocuSign_AuthToken(),
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id
            };
        }

        public static ActivityTemplateDTO Record_DocuSign_Envelope_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = 3,
                Name = "Record_DocuSign_Events_TEST",
                Version = "1"
            };
        }        

        public static ActivityDTO Send_DocuSign_Envelope_v1_Example_ActionDTO()
        {
            var activityTemplate = Send_DocuSign_Envelope_v1_ActivityTemplate();

            return new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Send DocuSign",
                AuthToken = DocuSign_AuthToken(),
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id
            };
        }

        public static ActivityTemplateDTO Mail_Merge_Into_DocuSign_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = 4,
                Name = "Mail_Merge_Into_DocuSign_TEST",
                Version = "1",                
            };
        }

        public static ActivityDTO Mail_Merge_Into_DocuSign_v1_InitialConfiguration_ActionDTO()
        {
            var activityTemplate = Mail_Merge_Into_DocuSign_v1_ActivityTemplate();

            return new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Mail Merge Into DocuSign",
                AuthToken = DocuSign_AuthToken(),
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id
            };
        }

        public static ActivityTemplateDTO Rich_Document_Notifications_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = 7,
                Name = "Rich_Document_Notifications_TEST",
                Version = "1"
            };
        }

        public static ActivityDTO Rich_Document_Notifications_v1_InitialConfiguration_ActionDTO()
        {
            var activityTemplate = Rich_Document_Notifications_v1_ActivityTemplate();

            return new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Rich Document Notifications",
                AuthToken = DocuSign_AuthToken(),
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id
            };
        }

        public static ActivityTemplateDTO Extract_Data_From_Envelopes_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = 4,
                Name = "Extract_Data_From_Envelopes_TEST",
                Version = "1"
            };
        }

        public static ActivityDTO Extract_Data_From_Envelopes_v1_InitialConfiguration_ActionDTO()
        {
            var activityTemplate = Extract_Data_From_Envelopes_v1_ActivityTemplate();

            return new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Extract Data From Envelopes",
                AuthToken = DocuSign_AuthToken(),
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id
            };
        }

        public static ActivityTemplateDTO Monitor_DocuSign_v1_ActivityTemplate_For_Solution()
        {
            return new ActivityTemplateDTO()
            {
                Id = 6,
                Name = "Monitor_DocuSign_Envelope_Activity",
                Version = "1",
                Label = "Monitor DocuSign Envelope Activity",
                Category = Data.States.ActivityCategory.Forwarders
            };
        }

        public static ActivityTemplateDTO Send_DocuSign_Envelope_v1_ActivityTemplate_for_Solution()
        {
            return new ActivityTemplateDTO()
            {
                Id = 5,
                Name = "Send_DocuSign_Envelope",
                Label = "Send DocuSign Envelope",
                Version = "1",
                Category = Data.States.ActivityCategory.Forwarders
            };
        }
    }
}
