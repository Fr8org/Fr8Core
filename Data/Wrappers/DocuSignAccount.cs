using DocuSign.Integrations.Client;
using Newtonsoft.Json.Linq;
using System.Configuration;
namespace Data.Wrappers
{
    public class DocuSignAccount : DocuSign.Integrations.Client.Account
    {
        DocuSignPackager _docuSignPackager;
        DocuSign.Integrations.Client.DocuSignConnect _docuSignConnect;
        private void DocuSignLogin()
        {
            _docuSignConnect = new DocuSignConnect();
            _docuSignPackager = new DocuSignPackager
            {
                CurrentEmail = ConfigurationManager.AppSettings["DocuSignLoginEmail"],
                CurrentApiPassword = ConfigurationManager.AppSettings["DocuSignLoginPassword"]
            };
            _docuSignConnect.Login = _docuSignPackager.Login();
        }
        public static DocuSignAccount Create(DocuSign.Integrations.Client.Account account)
        {
            return AutoMapper.Mapper.Map<DocuSignAccount>(account);
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