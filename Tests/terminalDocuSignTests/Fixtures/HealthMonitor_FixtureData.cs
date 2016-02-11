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

        public static Fr8DataDTO Monitor_DocuSign_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Monitor_DocuSign_v1_ActivityTemplate();

            var activity = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Monitor_DocuSign_Envelope_Activity",
                Label = "Monitor DocuSign Envelope Activity",
                AuthToken = DocuSign_AuthToken(),
                ActivityTemplate = activityTemplate
            };

            return ConvertToFr8Data(activity);
        }

        public static Fr8DataDTO Query_DocuSign_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Query_DocuSign_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Query_DocuSign",
                Label = "Query DocuSign",
                AuthToken = DocuSign_AuthToken(),
                ActivityTemplate = activityTemplate
            };

            return ConvertToFr8Data(activityDTO);
        }

        public static Fr8DataDTO Receive_DocuSign_Envelope_v1_Example_Fr8DataDTO()
        {
            var activityTemplate = Receive_DocuSign_Envelope_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Receive_DocuSign",
                Label = "Receive DocuSign",
                AuthToken = DocuSign_AuthToken(),
                ActivityTemplate = activityTemplate
            };

            return ConvertToFr8Data(activityDTO);
        }

        public static Fr8DataDTO Record_Docusign_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Record_DocuSign_Envelope_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Record_DocuSign",
                Label = "Record DocuSign",
                AuthToken = DocuSign_AuthToken(),
                ActivityTemplate = activityTemplate
            };
            return ConvertToFr8Data(activityDTO);
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

        public static Fr8DataDTO Send_DocuSign_Envelope_v1_Example_Fr8DataDTO()
        {
            var activityTemplate = Send_DocuSign_Envelope_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Send_DocuSign",
                Label = "Send DocuSign",
                AuthToken = DocuSign_AuthToken(),
                ActivityTemplate = activityTemplate
            };

            return ConvertToFr8Data(activityDTO);
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

        public static Fr8DataDTO Mail_Merge_Into_DocuSign_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Mail_Merge_Into_DocuSign_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Mail_Merge_Into_DocuSign",
                Label = "Mail Merge Into DocuSign",
                AuthToken = DocuSign_AuthToken(),
                ActivityTemplate = activityTemplate
            };
            return ConvertToFr8Data(activityDTO);
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

        public static Fr8DataDTO Rich_Document_Notifications_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Rich_Document_Notifications_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Rich_Document_Notifications",
                Label = "Rich Document Notifications",
                AuthToken = DocuSign_AuthToken(),
                ActivityTemplate = activityTemplate
            };

            return ConvertToFr8Data(activityDTO);
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

        public static Fr8DataDTO Extract_Data_From_Envelopes_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Extract_Data_From_Envelopes_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Extract_Data_From_Envelopes",
                Label = "Extract Data From Envelopes",
                AuthToken = DocuSign_AuthToken(),
                ActivityTemplate = activityTemplate
            };
            return ConvertToFr8Data(activityDTO);
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

        private static Fr8DataDTO ConvertToFr8Data(ActivityDTO activityDTO)
        {
            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }
    }
}
