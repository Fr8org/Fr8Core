using DocuSign.Integrations.Client;
using Newtonsoft.Json.Linq;
using System.Configuration;
using Utilities.Configuration.Azure;

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
    }
}