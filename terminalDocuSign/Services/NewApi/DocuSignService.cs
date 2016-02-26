using Data.Interfaces.DataTransferObjects;
using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using Newtonsoft.Json;
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

        public static void SendAnEnvelopeFromTemplate(DocuSignLoginInformation loginInfo, List<FieldDTO> rolesList, List<FieldDTO> fieldList, string curTemplateId)
        {
            //creatig an envelope
            EnvelopeDefinition envDef = new EnvelopeDefinition();
            envDef.EmailSubject = "Test message from Fr8";
            envDef.TemplateId = curTemplateId;
            envDef.Status = "created";

            //requesting it back to update status at the end
            EnvelopesApi envelopesApi = new EnvelopesApi((Configuration)loginInfo.Configuration);
            EnvelopeSummary envelopeSummary = envelopesApi.CreateEnvelope(loginInfo.AccountId, envDef);
            var envelope = envelopesApi.GetEnvelope(loginInfo.AccountId, envelopeSummary.EnvelopeId);


            var recepients = envelopesApi.ListRecipients(loginInfo.AccountId, envelopeSummary.EnvelopeId);

            //updating recipients
            foreach (var recepient in recepients.Signers)
            {
                var related_fields = rolesList.Where(a => a.Tags.Contains("recipientId:" + recepient.RecipientId));
                recepient.Name = related_fields.Where(a => a.Key.Contains("role name")).FirstOrDefault().Value;
                recepient.Email = related_fields.Where(a => a.Key.Contains("role email")).FirstOrDefault().Value;

                //updating tabs
                var tabs = envelopesApi.ListTabs(loginInfo.AccountId, envelope.EnvelopeId, recepient.RecipientId);
                JObject jobj = JObject.Parse(tabs.ToJson());
                foreach (var item in jobj.Properties())
                {
                    string tab_type = item.Name;
                    var fields = fieldList.Where(a => a.Tags.Contains(tab_type) && a.Tags.Contains("recipientId:" + recepient.RecipientId));
                    foreach (JObject tab in item.Value)
                    {
                        FieldDTO corresponding_field = null;
                        switch (tab_type)
                        {
                            case "radioGroupTabs":
                                corresponding_field = fields.Where(a => a.Key.Contains(tab.Property("groupName").Value.ToString())).FirstOrDefault();
                                if (corresponding_field == null)
                                    break;
                                int index = Convert.ToInt32(corresponding_field.Value);
                                tab["radios"].ElementAt(index)["selected"] = "true";
                                break;

                            case "listTabs":
                                corresponding_field = fields.Where(a => a.Key.Contains(tab.Property("tabLabel").Value.ToString())).FirstOrDefault();
                                if (corresponding_field == null)
                                    break;
                                tab["listItems"].Where(a => a["value"].ToString() == corresponding_field.Value).FirstOrDefault()["selected"] = "true";
                                tab["value"] = corresponding_field.Value;
                                break;

                            default:
                                corresponding_field = fields.Where(a => a.Key.Contains(tab.Property("tabLabel").Value.ToString())).FirstOrDefault();
                                if (corresponding_field == null)
                                    break;
                                tab["value"] = corresponding_field.Value;
                                break;
                        }
                    }
                }

                tabs = jobj.ToObject<Tabs>();

                envelopesApi.UpdateTabs(loginInfo.AccountId, envelope.EnvelopeId, recepient.RecipientId, tabs);
                //end of tabs updating
            }

            envelopesApi.UpdateRecipients(loginInfo.AccountId, envelope.EnvelopeId, recepients);

            envelope.Status = "sent";
            envelope.PurgeState = "";

            envelopesApi.Update(loginInfo.AccountId, envelope.EnvelopeId, envelope);
        }
    }
}