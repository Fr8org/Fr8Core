
namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static DocuSign.Integrations.Client.Configuration TestCreateConnectProfile1()
        {
            var actionTemplate = ActivityTemplate();

            var curConfiguration = new DocuSign.Integrations.Client.Configuration
            {
                configurationType = "custom",
                urlToPublishTo = "http://0347dd6f.ngrok.io/docusign_notifications",
                name = "Dockyard_2",
                allowEnvelopePublish = "true",
                enableLog = "false",
                includeDocuments = "true",
                includeCertificateOfCompletion = "false",
                requiresAcknowledgement = "false",
                signMessageWithX509Certificate = "false",
                useSoapInterface = "false",
                includeTimeZoneInformation = "false",
                includeEnvelopeVoidReason = "false",
                includeSenderAccountasCustomField = "false",
                envelopeEvents = "Completed,Declined,Delivered,Sent,Voided",
                recipientEvents = "Completed,Declined,Delivered,Sent,AuthenticationFailed,AutoResponded",
                userIds = "64684b41-bdfd-4121-8f81-c825a6a03582",
                soapNamespace = "",
                allUsers = "false",
                includeCertSoapHeader = "false",
                includeDocumentFields = "true"

            };
            return curConfiguration;
        }
    }
}