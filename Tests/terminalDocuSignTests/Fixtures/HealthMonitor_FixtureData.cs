using System;
using Data.Interfaces.DataTransferObjects;
using NUnit.Framework;
using terminalDocuSignTests.Integration;
using HealthMonitor.Utility;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace terminalDocuSignTests.Fixtures
{
    public class HealthMonitor_FixtureData
    {
        private static AuthorizationTokenDTO DocuSignToken;

        public static AuthorizationTokenDTO DocuSign_AuthToken(BaseIntegrationTest integrationTest)
        {
            if (DocuSignToken == null)
            {
                var creds = new CredentialsDTO()
                {
                    Username = "freight.testing@gmail.com",
                    Password = "I6HmXEbCxN",
                    IsDemoAccount = true
                };

                string endpoint = integrationTest.GetTerminalUrl() + "/authentication/internal";
                var task = integrationTest.HttpPostAsync<CredentialsDTO, JObject>(endpoint, creds);
                DocuSignToken = JsonConvert.DeserializeObject<AuthorizationTokenDTO>(task.Result.ToString());
                Assert.IsTrue(string.IsNullOrEmpty(DocuSignToken.Error));
            }

            return DocuSignToken;
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

        public static Fr8DataDTO Monitor_DocuSign_v1_InitialConfiguration_Fr8DataDTO(BaseIntegrationTest integrationTest)
        {
            var activityTemplate = Monitor_DocuSign_v1_ActivityTemplate();

            var activity = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Monitor DocuSign Envelope Activity",
                AuthToken = DocuSign_AuthToken(integrationTest),
                ActivityTemplate = activityTemplate
            };

            return ConvertToFr8Data(activity);
        }

        public static Fr8DataDTO Query_DocuSign_v1_InitialConfiguration_Fr8DataDTO(BaseIntegrationTest integrationTest)
        {
            var activityTemplate = Query_DocuSign_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Query DocuSign",
                AuthToken = DocuSign_AuthToken(integrationTest),
                ActivityTemplate = activityTemplate
            };

            return ConvertToFr8Data(activityDTO);
        }

        public static Fr8DataDTO Receive_DocuSign_Envelope_v1_Example_Fr8DataDTO(BaseIntegrationTest integrationTest)
        {
            var activityTemplate = Receive_DocuSign_Envelope_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Receive DocuSign",
                AuthToken = DocuSign_AuthToken(integrationTest),
                ActivityTemplate = activityTemplate
            };

            return ConvertToFr8Data(activityDTO);
        }

        public static Fr8DataDTO Record_Docusign_v1_InitialConfiguration_Fr8DataDTO(BaseIntegrationTest integrationTest)
        {
            var activityTemplate = Record_DocuSign_Envelope_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Record DocuSign",
                AuthToken = DocuSign_AuthToken(integrationTest),
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

        public static Fr8DataDTO Send_DocuSign_Envelope_v1_Example_Fr8DataDTO(BaseIntegrationTest integrationTest)
        {
            var activityTemplate = Send_DocuSign_Envelope_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Send DocuSign",
                AuthToken = DocuSign_AuthToken(integrationTest),
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


        public static Fr8DataDTO Mail_Merge_Into_DocuSign_v1_InitialConfiguration_Fr8DataDTO(BaseIntegrationTest integrationTest)
        {
            var activityTemplate = Mail_Merge_Into_DocuSign_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Mail Merge Into DocuSign",
                AuthToken = DocuSign_AuthToken(integrationTest),
                ActivityTemplate = activityTemplate
            };
            return ConvertToFr8Data(activityDTO);
        }

        public static ActivityTemplateDTO Track_DocuSign_Recipients_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = 7,
                Name = "Track_DocuSign_Recipients_TEST",
                Version = "1"
            };
        }

        public static Fr8DataDTO Track_DocuSign_Recipients_v1_InitialConfiguration_Fr8DataDTO(BaseIntegrationTest integrationTest)
        {
            var activityTemplate = Track_DocuSign_Recipients_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Track DocuSign Recipients",
                AuthToken = DocuSign_AuthToken(integrationTest),
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

        public static Fr8DataDTO Extract_Data_From_Envelopes_v1_InitialConfiguration_Fr8DataDTO(BaseIntegrationTest integrationTest)
        {
            var activityTemplate = Extract_Data_From_Envelopes_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Extract Data From Envelopes",
                AuthToken = DocuSign_AuthToken(integrationTest),
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
