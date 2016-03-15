using Data.Control;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
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
using terminalDocuSign.Services.NewApi;
using Utilities.Configuration.Azure;

namespace terminalDocuSign.Services.New_Api
{

    public class DocuSignApiConfiguration
    {
        public string AccountId;
        public dynamic Configuration;
    }

    public class DocuSignService
    {
        public static DocuSignApiConfiguration SetUp(AuthorizationTokenDO authTokenDO)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);
            //create configuration for future api calls
            string baseUrl = CloudConfigurationManager.GetSetting("environment") + "restapi/";
            string integratorKey = CloudConfigurationManager.GetSetting("DocuSignIntegratorKey");
            ApiClient apiClient = new ApiClient(baseUrl);
            string authHeader = "bearer " + docuSignAuthDTO.ApiPassword;
            Configuration conf = new Configuration(apiClient);
            conf.AddDefaultHeader("Authorization", authHeader);
            DocuSignApiConfiguration result = new DocuSignApiConfiguration() { AccountId = docuSignAuthDTO.AccountId, Configuration = conf };

            if (string.IsNullOrEmpty(docuSignAuthDTO.AccountId)) //we deal with and old token, that don't have accountId yet
            {
                AuthenticationApi authApi = new AuthenticationApi(conf);
                LoginInformation loginInfo = authApi.Login();
                result.AccountId = loginInfo.LoginAccounts[0].AccountId; //it seems that althought one DocuSign account can have multiple users - only one is returned, the one that oAuth token was created for
            }

            return result;
        }

        public static List<FieldDTO> GetTemplatesList(DocuSignApiConfiguration conf)
        {
            var tmpApi = new TemplatesApi(conf.Configuration);
            var result = tmpApi.ListTemplates(conf.AccountId);
            if (result.EnvelopeTemplates != null && result.EnvelopeTemplates.Count > 0)
                return result.EnvelopeTemplates.Where(a => !string.IsNullOrEmpty(a.Name))
                    .Select(a => new FieldDTO(a.Name, a.TemplateId) { Availability = AvailabilityType.Configuration }).ToList();
            else
                return new List<FieldDTO>();
        }

        public static IEnumerable<FieldDTO> GetFolders(DocuSignApiConfiguration conf)
        {
            FoldersApi api = new FoldersApi(conf.Configuration);
            var folders = api.List(conf.AccountId);
            if (folders.Folders != null)
                return folders.Folders.Select(a => new FieldDTO(a.Name, a.Name));
            else
                return new List<FieldDTO>();
        }

        public static DocuSignTemplateDTO DownloadDocuSignTemplate(DocuSignApiConfiguration config, string selectedDocusignTemplateId)
        {
            // we probably need to make multiple calls to api to collect all template info, i.e. recipients, tabs etc.
            //return Mapper.Map<DocuSignTemplateDTO>(jObjTemplate);

            throw new NotImplementedException();
        }

        public static IEnumerable<FieldDTO> GetEnvelopeRecipientsAndTabs(DocuSignApiConfiguration conf, string envelopeId)
        {
            var envApi = new EnvelopesApi(conf.Configuration);
            return GetRecipientsAndTabs(conf, envApi, envelopeId);
        }

        public static IEnumerable<FieldDTO> GetTemplateRecipientsAndTabs(DocuSignApiConfiguration conf, string templateId)
        {
            var tmpApi = new TemplatesApi(conf.Configuration);
            return GetRecipientsAndTabs(conf, tmpApi, templateId);
        }

        #region GenerateDocuSignReport methods

        public static int CountEnvelopes(DocuSignApiConfiguration config, DocusignQuery docusignQuery)
        {
            throw new NotImplementedException();
        }

        public static object SearchDocuSign(DocuSignApiConfiguration config, List<FilterConditionDTO> conditions, HashSet<string> existing_envelopes, StandardPayloadDataCM search_result)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Send DocuSign Envelope methods

        //this is purely for Send_DocuSign_Envelope activity
        public static Tuple<IEnumerable<FieldDTO>, IEnumerable<DocuSignTabDTO>> GetTemplateRecipientsTabsAndDocuSignTabs(DocuSignApiConfiguration conf, string templateId)
        {
            var tmpApi = new TemplatesApi(conf.Configuration);
            var recipientsAndTabs = new List<FieldDTO>();
            var docuTabs = new List<DocuSignTabDTO>();

            var recipients = GetRecipients(conf, tmpApi, templateId);
            recipientsAndTabs.AddRange(MapRecipientsToFieldDTO(recipients));

            foreach (var signer in recipients.Signers)
            {
                var tabs = tmpApi.ListTabs(conf.AccountId, templateId, signer.RecipientId, new Tabs());
                var signersdocutabs = DocuSignTab.ExtractTabs(JObject.Parse(tabs.ToJson()), signer.RoleName).ToList();
                docuTabs.AddRange(signersdocutabs);
                recipientsAndTabs.AddRange(DocuSignTab.MapTabsToFieldDTO(signersdocutabs));
            }

            recipientsAndTabs.ForEach(a => a.Availability = AvailabilityType.RunTime);

            return new Tuple<IEnumerable<FieldDTO>, IEnumerable<DocuSignTabDTO>>(recipientsAndTabs, docuTabs);
        }

        public static void SendAnEnvelopeFromTemplate(DocuSignApiConfiguration loginInfo, List<FieldDTO> rolesList, List<FieldDTO> fieldList, string curTemplateId)
        {

            //creatig an envelope definiton
            EnvelopeDefinition envDef = new EnvelopeDefinition();
            envDef.EmailSubject = "Test message from Fr8";
            envDef.TemplateId = curTemplateId;
            envDef.Status = "created";

            //creating an envelope
            EnvelopesApi envelopesApi = new EnvelopesApi(loginInfo.Configuration);
            EnvelopeSummary envelopeSummary = envelopesApi.CreateEnvelope(loginInfo.AccountId, envDef);


            var templatesApi = new TemplatesApi(loginInfo.Configuration);
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
                                tab["radios"].Where(a => a["value"].ToString() == corresponding_field.Value).FirstOrDefault()["selected"] = "true";
                                foreach (var radioItem in tab["radios"].Where(a => a["value"].ToString() != corresponding_field.Value).ToList())
                                {
                                    radioItem["selected"] = "false";
                                }
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

        #endregion

        #region private methods

        private static IEnumerable<FieldDTO> GetRecipientsAndTabs(DocuSignApiConfiguration conf, dynamic api, string id)
        {
            var result = new List<FieldDTO>();
            var recipients = GetRecipients(conf, api, id);
            result.AddRange(MapRecipientsToFieldDTO(recipients));
            foreach (var recipient in recipients.Signers)
            {
                result.AddRange(GetTabs(conf, api, id, recipient));
            }

            return result;
        }

        private static Recipients GetRecipients(DocuSignApiConfiguration conf, dynamic api, string id)
        {
            return api.ListRecipients(conf.AccountId, id) as Recipients;
        }

        private static IEnumerable<FieldDTO> GetTabs(DocuSignApiConfiguration conf, dynamic api, string id, Signer recipient)
        {
            var docutabs = (api is EnvelopesApi) ?
                       api.ListTabs(conf.AccountId, id, recipient.RecipientId)
                       : api.ListTabs(conf.AccountId, id, recipient.RecipientId, new Tabs());

            return (DocuSignTab.GetEnvelopeTabsPerSigner(JObject.Parse(docutabs.ToJson()), recipient.RoleName));
        }

        private static IEnumerable<FieldDTO> MapRecipientsToFieldDTO(Recipients recipients)
        {
            var result = new List<FieldDTO>();
            if (recipients.Signers != null)
                recipients.Signers.ForEach(
                    a =>
                    {
                        //use RoleName. If unavailable use a Name. If unavaible use email
                        result.Add(new FieldDTO((a.RoleName ?? a.Name ?? a.Email) + " role name", a.Name) { Tags = "DocuSigner, recipientId:" + a.RecipientId });
                        result.Add(new FieldDTO((a.RoleName ?? a.Name ?? a.Email) + " role email", a.Email) { Tags = "DocuSigner, recipientId:" + a.RecipientId });
                    });
            return result;
        }

        #endregion

    }
}
