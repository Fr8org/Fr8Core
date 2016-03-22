using DocuSign.Integrations.Client;
using Newtonsoft.Json.Linq;
using System.Configuration;
using Utilities.Configuration.Azure;
using System.Linq;
namespace terminalDocuSign.Infrastructure
{
    public class DocuSignAccount : Account
    {
        DocuSignPackager _docuSignPackager;
        DocuSignConnect _docuSignConnect;
        private void DocuSignLogin()
        {
            _docuSignConnect = new DocuSignConnect();
            _docuSignPackager = new DocuSignPackager
            {
                CurrentEmail = CloudConfigurationManager.GetSetting("DocuSignLoginEmail"),
                CurrentApiPassword = CloudConfigurationManager.GetSetting("DocuSignLoginPassword")
            };
            _docuSignConnect.Login = _docuSignPackager.Login();
        }
        public ConnectProfile GetDocuSignConnectProfiles()
        {
            DocuSignLogin();
            return _docuSignConnect.Get();
        }

		public DocuSign.Integrations.Client.Configuration CreateDocuSignConnectProfile(DocuSign.Integrations.Client.Configuration configuration)
        {
            DocuSignLogin();
            return _docuSignConnect.Create(configuration);
        }
		  public DocuSign.Integrations.Client.Configuration UpdateDocuSignConnectProfile(DocuSign.Integrations.Client.Configuration configuration)
        {
            DocuSignLogin();
            return _docuSignConnect.Update(configuration);
        }

        public void DeleteDocuSignConnectProfile(string connectId)
        {
            DocuSignLogin();
            _docuSignConnect.Delete(connectId);
        }

        public static void CreateOrUpdateDefaultDocuSignConnectConfiguration(string envelopeEvents = "Sent, Delivered, Completed")
        {
            var account = new DocuSignAccount();
            var publishUrl = "http://" + CloudConfigurationManager.GetSetting("terminalDocuSign.TerminalEndpoint") + "/terminals/terminalDocuSign/events";

            //get existing connect configuration
            var existingDocuSignConnectConfiguration = GetDocuSignConnectConfiguration(account);

            if (existingDocuSignConnectConfiguration == null)
            {
                //if existing configuration is not present, create one
                account.CreateDocuSignConnectProfile(new DocuSign.Integrations.Client.Configuration
                {
                    name = "fr8DocuSignConnectConfiguration",
                    allUsers = "true",
                    configurationType = "custom",
                    allowEnvelopePublish = "true",
                    envelopeEvents = envelopeEvents,
                    urlToPublishTo = publishUrl,
                    enableLog = "true",
                    includeDocuments = "false",
                    requiresAcknowledgement = "false",
                    includeCertSoapHeader = "false",
                    includeCertificateOfCompletion = "false",
                    includeTimeZoneInformation = "false",
                    includeDocumentFields = "false",
                    includeEnvelopeVoidReason = "true",
                    includeSenderAccountasCustomField = "false",
                    recipientEvents = "",
                    useSoapInterface = "false",
                    signMessageWithX509Certificate = "false"
                });
            }
            else
            {
                //update existing configuration with new envelope events and publish URL
                if(envelopeEvents != null)
                    existingDocuSignConnectConfiguration.envelopeEvents = envelopeEvents;
                existingDocuSignConnectConfiguration.urlToPublishTo = publishUrl;
                account.UpdateDocuSignConnectProfile(existingDocuSignConnectConfiguration);
            }
        }

        private static DocuSign.Integrations.Client.Configuration GetDocuSignConnectConfiguration(DocuSignAccount account)
        {
            //get all connect profiles from DocuSign for the given account
            var connectProfile = account.GetDocuSignConnectProfiles();

            //if DocuSignConnectName is already present, return the config
            if (connectProfile.configurations.Any(config => config.name == "fr8DocuSignConnectConfiguration"))
            {
                return connectProfile.configurations.First(config => config.name == "fr8DocuSignConnectConfiguration");
            }

            //if nothing found, return NULL
            return null;
        }
    }
}