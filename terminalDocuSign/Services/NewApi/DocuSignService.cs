using Data.Interfaces.DataTransferObjects;
using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Utilities.Configuration.Azure;

namespace terminalDocuSign.Services.New_Api
{

    public class DocuSignLoginInformation
    {
        public string AccountId;
        public object Configuration;
    }

    public class DocuSignService
    {
        public static DocuSignLoginInformation Login(string email, string password)
        {
            string baseUrl = CloudConfigurationManager.GetSetting("environment") + "restapi/";
            string integratorKey = CloudConfigurationManager.GetSetting("DocuSignIntegratorKey");
            ApiClient apiClient = new ApiClient(baseUrl);
            string authHeader = "{\"Username\":\"" + email + "\", \"Password\":\"" + password + "\", \"IntegratorKey\":\"" + integratorKey + "\"}";
            Configuration conf = new Configuration(apiClient);
            conf.AddDefaultHeader("X-DocuSign-Authentication", authHeader);
            conf.Username = email;
            conf.Password = password;

            AuthenticationApi authApi = new AuthenticationApi(conf);
            LoginInformation loginInfo = authApi.Login();
            return new DocuSignLoginInformation() { AccountId = loginInfo.LoginAccounts[0].AccountId, Configuration = conf };
        }

        public static List<FieldDTO> GetRolesAndTabs(DocuSignLoginInformation login, string templateId)
        {
            var result = new List<FieldDTO>();
            var templatesApi = new TemplatesApi((Configuration)login.Configuration);
            var recepients = templatesApi.ListRecipients(login.AccountId, templateId);

            foreach (var signer in recepients.Signers)
            {
                result.Add(
                    new FieldDTO(string.Format("{0} role name", signer.RoleName), signer.RecipientId));

                result.Add(
                    new FieldDTO(string.Format("{0} role email", signer.RoleName), signer.RecipientId));

                //handling tabs
                var tabs = templatesApi.ListTabs(login.AccountId, templateId, signer.RecipientId, new Tabs());
                JObject jobj = JObject.Parse(tabs.ToJson());
                foreach (var item in jobj.Properties())
                {
                    string tab_type = item.Name;
                    foreach (JObject tab in item.Value)
                    {
                        string value = tab.Value<string>("value");
                        string label = tab.Value<string>("tabLabel");

                        if (!string.IsNullOrEmpty(label) && value != null)
                        {
                            result.Add(new FieldDTO(
                                string.Format("{0} ({1})", label, signer.RoleName),
                                tab_type
                            ));
                        }
                    }
                }
            }

            return result;
        }

    }
}