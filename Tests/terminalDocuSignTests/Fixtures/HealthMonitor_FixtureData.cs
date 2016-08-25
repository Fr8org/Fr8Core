using System;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using NUnit.Framework;
using Fr8.Testing.Integration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using terminalDocuSign.Infrastructure;

namespace terminalDocuSignTests.Fixtures
{
    public class HealthMonitor_FixtureData
    {
        private static AuthorizationTokenDTO DocuSignToken;

        public static async Task<AuthorizationTokenDTO> DocuSign_AuthToken(BaseIntegrationTest integrationTest)
        {
            if (DocuSignToken == null)
            {
                var creds = new CredentialsDTO()
                {
                    Username = "freight.testing@gmail.com",
                    Password = "I6HmXEbCxN",
                    IsDemoAccount = true
                };

                string endpoint = integrationTest.GetTerminalUrl() + "/authentication/token";
                var jobject = await integrationTest.HttpPostAsync<CredentialsDTO, JObject>(endpoint, creds);
                DocuSignToken = JsonConvert.DeserializeObject<AuthorizationTokenDTO>(jobject.ToString());
                Assert.IsTrue(string.IsNullOrEmpty(DocuSignToken.Error));
            }

            return DocuSignToken;
        }

        public static ActivityTemplateSummaryDTO Monitor_DocuSign_v1_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Monitor_DocuSign_Envelope_Activity_TEST",
                Version = "1"
            };
        }

        public static ActivityTemplateSummaryDTO Query_DocuSign_v1_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Query_DocuSign_TEST",
                Version = "1"
            };
        }

        public static ActivityTemplateSummaryDTO Receive_DocuSign_Envelope_v1_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Receive_DocuSign_Envelope_TEST",
                Version = "1"
            };
        }

        public static ActivityTemplateSummaryDTO Send_DocuSign_Envelope_v1_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Send_DocuSign_Envelope_TEST",
                Version = "1"
            };
        }

        public static async Task<Fr8DataDTO> Monitor_DocuSign_v1_InitialConfiguration_Fr8DataDTO(BaseIntegrationTest integrationTest)
        {
            var activityTemplate = Monitor_DocuSign_v1_ActivityTemplate();

            var activity = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Monitor DocuSign Envelope Activity",
                AuthToken = await DocuSign_AuthToken(integrationTest),
                ActivityTemplate = activityTemplate
            };

            return ConvertToFr8Data(activity);
        }

        public static async Task<Fr8DataDTO> Query_DocuSign_v1_InitialConfiguration_Fr8DataDTO(BaseIntegrationTest integrationTest)
        {
            var activityTemplate = Query_DocuSign_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Query DocuSign",
                AuthToken = await DocuSign_AuthToken(integrationTest),
                ActivityTemplate = activityTemplate
            };

            return ConvertToFr8Data(activityDTO);
        }

        public static async Task<Fr8DataDTO> Receive_DocuSign_Envelope_v1_Example_Fr8DataDTO(BaseIntegrationTest integrationTest)
        {
            var activityTemplate = Receive_DocuSign_Envelope_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Receive DocuSign",
                AuthToken = await DocuSign_AuthToken(integrationTest),
                ActivityTemplate = activityTemplate
            };

            return ConvertToFr8Data(activityDTO);
        }

        public static async Task<Fr8DataDTO> Prepare_DocuSign_Events_For_Storage_v1_InitialConfiguration_Fr8DataDTO(BaseIntegrationTest integrationTest)
        {
            var activityTemplate = Record_DocuSign_Envelope_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Record DocuSign",
                AuthToken = await DocuSign_AuthToken(integrationTest),
                ActivityTemplate = activityTemplate
            };
            return ConvertToFr8Data(activityDTO);
        }

        public static ActivityTemplateSummaryDTO Record_DocuSign_Envelope_v1_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Prepare_DocuSign_Events_For_Storage_TEST",
                Version = "1"
            };
        }

        public static async Task<Fr8DataDTO> Send_DocuSign_Envelope_v1_Example_Fr8DataDTO(BaseIntegrationTest integrationTest)
        {
            var activityTemplate = Send_DocuSign_Envelope_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Send DocuSign",
                AuthToken = await DocuSign_AuthToken(integrationTest),
                ActivityTemplate = activityTemplate
            };

            return ConvertToFr8Data(activityDTO);
        }

        public static ActivityTemplateSummaryDTO Mail_Merge_Into_DocuSign_v1_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Mail_Merge_Into_DocuSign_TEST",
                Version = "1",
            };
        }


        public static async Task<Fr8DataDTO> Mail_Merge_Into_DocuSign_v1_InitialConfiguration_Fr8DataDTO(BaseIntegrationTest integrationTest)
        {
            var activityTemplate = Mail_Merge_Into_DocuSign_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Mail Merge Into DocuSign",
                AuthToken = await DocuSign_AuthToken(integrationTest),
                ActivityTemplate = activityTemplate
            };
            return ConvertToFr8Data(activityDTO);
        }

        public static ActivityTemplateSummaryDTO Track_DocuSign_Recipients_v1_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Track_DocuSign_Recipients_TEST",
                Version = "1"
            };
        }

        public static async Task<Fr8DataDTO> Track_DocuSign_Recipients_v1_InitialConfiguration_Fr8DataDTO(BaseIntegrationTest integrationTest)
        {
            var activityTemplate = Track_DocuSign_Recipients_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Track DocuSign Recipients",
                AuthToken = await DocuSign_AuthToken(integrationTest),
                ActivityTemplate = activityTemplate
            };

            return ConvertToFr8Data(activityDTO);
        }

        public static ActivityTemplateSummaryDTO Extract_Data_From_Envelopes_v1_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Extract_Data_From_Envelopes_TEST",
                Version = "1"
            };
        }

        public static async Task<Fr8DataDTO> Extract_Data_From_Envelopes_v1_InitialConfiguration_Fr8DataDTO(BaseIntegrationTest integrationTest)
        {
            var activityTemplate = Extract_Data_From_Envelopes_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Extract Data From Envelopes",
                AuthToken = await DocuSign_AuthToken(integrationTest),
                ActivityTemplate = activityTemplate
            };
            return ConvertToFr8Data(activityDTO);
        }

        public static ActivityTemplateDTO Monitor_DocuSign_v1_ActivityTemplate_For_Solution()
        {
            return new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Monitor_DocuSign_Envelope_Activity",
                Version = "1",
                Label = "Monitor DocuSign Envelope Activity",
                Categories = new[] { ActivityCategories.Forward }
            };
        }

        public static ActivityTemplateDTO Send_DocuSign_Envelope_v1_ActivityTemplate_for_Solution()
        {
            return new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Send_DocuSign_Envelope",
                Label = "Send DocuSign Envelope",
                Version = "1",
                Categories = new [] { ActivityCategories.Forward }
            };
        }

        private static Fr8DataDTO ConvertToFr8Data(ActivityDTO activityDTO)
        {
            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }

        public static ICrateStorage GetEnvelopePayload()
        {
            var curDocuSignEnvelopeInfo = DocuSignEventParser.GetEnvelopeInformation(EnvelopePayload);
            var content = DocuSignEventParser.ParseXMLintoCM(curDocuSignEnvelopeInfo);
            return new CrateStorage(Crate.FromContent("DocuSign Connect Event", content));
        }

        private static string EnvelopePayload = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
            <DocuSignEnvelopeInformation xmlns = ""http://www.docusign.net/API/3.0"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
               <EnvelopeStatus>
                  <RecipientStatuses>
                     <RecipientStatus>
                        <Type>CertifiedDelivery</Type>
                        <Email>hal9000@discovery.com</Email>
                        <UserName>HAL-9000</UserName>
                        <RoutingOrder>1</RoutingOrder>
                        <Sent>2015-09-29T07:38:22.653</Sent>
                        <DeclineReason xsi:nil= ""true"" />
                        <Status>Sent</Status>
                        <RecipientIPAddress/>
                        <CustomFields/>
                        <AccountStatus>Active</AccountStatus>
                        <RecipientId>279a1173-04cc-4902-8039-68b1992639e9</RecipientId>
                     </RecipientStatus>
                  </RecipientStatuses>
                  <TimeGenerated>2015-09-29T07:38:42.7464809</TimeGenerated>
                  <EnvelopeID>e1233907-33dc-4d40-b637-3a1f4a8f0ff8</EnvelopeID>
                  <Subject>Open the Pod bay doors, HAL</Subject>
                  <UserName>Dave Bowman</UserName>
                  <Email>fr8.madse.testing@gmail.com</Email>
                  <Status>Sent</Status>
                  <Created>2015-09-29T07:37:42.813</Created>
                  <Sent>2015-09-29T07:38:22.7</Sent>
                  <ACStatus>Original</ACStatus>
                  <ACStatusDate>2015-09-29T07:37:42.813</ACStatusDate>
                  <ACHolder>Dave Bowman</ACHolder>
                  <ACHolderEmail>fr8.madse.testing@gmail.com</ACHolderEmail>
                  <ACHolderLocation>DocuSign</ACHolderLocation>
                  <SigningLocation>Online</SigningLocation>
                  <SenderIPAddress>10.103.101.11</SenderIPAddress>
                  <EnvelopePDFHash />
                  <CustomFields />
                  <AutoNavigation>true</AutoNavigation>
                  <EnvelopeIdStamping>true</EnvelopeIdStamping>
                  <AuthoritativeCopy>false</AuthoritativeCopy>
                  <DocumentStatuses>
                     <DocumentStatus>
                        <ID>85548272</ID>
                        <Name>image.jpg</Name>
                        <TemplateName>Fr8 Fromentum Registration Form</TemplateName>
                        <Sequence>1</Sequence>
                     </DocumentStatus>
                  </DocumentStatuses>
               </EnvelopeStatus>
            </DocuSignEnvelopeInformation>";


    }
}
