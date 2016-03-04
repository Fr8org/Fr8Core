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
using terminalDocuSign.DataTransferObjects;
using Utilities.Configuration.Azure;

namespace terminalDocuSign.Services.New_Api
{

    public class DocuSignApiConfiguration
    {
        public string AccountId;
        public object Configuration;
    }

    public class DocuSignService
    {
        public static DocuSignApiConfiguration SetUp(DocuSignAuthTokenDTO authToken)
        {
            //create configuration for future api calls
            string baseUrl = CloudConfigurationManager.GetSetting("environment") + "restapi/";
            string integratorKey = CloudConfigurationManager.GetSetting("DocuSignIntegratorKey");
            ApiClient apiClient = new ApiClient(baseUrl);
            string authHeader = "bearer " + authToken.ApiPassword;
            Configuration conf = new Configuration(apiClient);
            conf.AddDefaultHeader("Authorization", authHeader);
            DocuSignApiConfiguration result = new DocuSignApiConfiguration() { AccountId = authToken.AccountId, Configuration = conf };

            if (string.IsNullOrEmpty(authToken.AccountId)) //we deal with and old token, that don't have accountId yet
            {
                AuthenticationApi authApi = new AuthenticationApi(conf);
                LoginInformation loginInfo = authApi.Login();
                result.AccountId = loginInfo.LoginAccounts[0].AccountId; //it seems that althought one DocuSign account can have multiple users - only one is returned, the one that oAuth token was created for
            }

            return result;
        }


        public static List<FieldDTO> GetRolesAndTabs(DocuSignApiConfiguration apiConfiguration, string templateId)
        {
            var result = new List<FieldDTO>();
            var templatesApi = new TemplatesApi((Configuration)apiConfiguration.Configuration);
            var recepients = templatesApi.ListRecipients(apiConfiguration.AccountId, templateId);

            foreach (var signer in recepients.Signers)
            {
                result.Add(
                    new FieldDTO(string.Format("{0} role name", signer.RoleName), signer.RecipientId));

                result.Add(
                    new FieldDTO(string.Format("{0} role email", signer.RoleName), signer.RecipientId));

                //handling tabs
                var tabs = templatesApi.ListTabs(apiConfiguration.AccountId, templateId, signer.RecipientId, new Tabs());
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

        public static void SendAnEnvelopeFromTemplate(DocuSignApiConfiguration loginInfo, List<FieldDTO> rolesList, List<FieldDTO> fieldList, string curTemplateId)
        {
            //creatig an envelope
            EnvelopeDefinition envDef = new EnvelopeDefinition();
            envDef.EmailSubject = "Test message from Fr8";
            envDef.TemplateId = curTemplateId;
            envDef.Status = "created";

            //requesting it back to update status at the end
            EnvelopesApi envelopesApi = new EnvelopesApi((Configuration)loginInfo.Configuration);
            EnvelopeSummary envelopeSummary = envelopesApi.CreateEnvelope(loginInfo.AccountId, envDef);


            var templatesApi = new TemplatesApi((Configuration)loginInfo.Configuration);
            var templateRecepients = templatesApi.ListRecipients(loginInfo.AccountId, curTemplateId);

            var recepients = envelopesApi.ListRecipients(loginInfo.AccountId, envelopeSummary.EnvelopeId);

            //updating recipients
            foreach (var recepient in recepients.Signers)
            {
                var corresponding_template_recipient = templateRecepients.Signers.Where(a => a.RoutingOrder == recepient.RoutingOrder).FirstOrDefault();
                var related_fields = rolesList.Where(a => a.Tags.Contains("recipientId:" + corresponding_template_recipient.RecipientId));
                recepient.Name = related_fields.Where(a => a.Key.Contains("role name")).FirstOrDefault().Value;
                recepient.Email = related_fields.Where(a => a.Key.Contains("role email")).FirstOrDefault().Value;

                //updating tabs
                var tabs = envelopesApi.ListTabs(loginInfo.AccountId, envelopeSummary.EnvelopeId, recepient.RecipientId);
                JObject jobj = JObject.Parse(tabs.ToJson());
                foreach (var item in jobj.Properties())
                {
                    string tab_type = item.Name;
                    var fields = fieldList.Where(a => a.Tags.Contains(tab_type) && a.Tags.Contains("recipientId:" + corresponding_template_recipient.RecipientId));
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
                                tab["listItems"].Where(a => a["value"].ToString() == corresponding_field.Value.Trim()).FirstOrDefault()["selected"] = "true";
                                foreach (var listItem in tab["listItems"].Where(a => a["value"].ToString() != corresponding_field.Value.Trim()))
                                {
                                    //set all other to false
                                    listItem["selected"] = "false";
                                }
                                //["selected"] = "true";
                                tab["value"] = corresponding_field.Value;
                                break;
                            case "checkboxTabs":
                                corresponding_field = fields.Where(a => a.Key.Contains(tab.Property("tabLabel").Value.ToString())).FirstOrDefault();
                                if (corresponding_field == null)
                                    break;
                                tab["selected"] = corresponding_field.Value;
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

                envelopesApi.UpdateTabs(loginInfo.AccountId, envelopeSummary.EnvelopeId, recepient.RecipientId, tabs);
                //end of tabs updating
            }

            envelopesApi.UpdateRecipients(loginInfo.AccountId, envelopeSummary.EnvelopeId, recepients);



            envelopesApi.Update(loginInfo.AccountId, envelopeSummary.EnvelopeId, new Envelope() { Status = "sent" });
        }
    }
}
