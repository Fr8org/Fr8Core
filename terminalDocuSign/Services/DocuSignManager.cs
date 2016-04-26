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
using Data.Validations;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services.NewApi;
using Utilities.Configuration.Azure;
using System.IO;

namespace terminalDocuSign.Services.New_Api
{
    public class DocuSignApiConfiguration
    {
        public string AccountId;
        public Configuration Configuration;
    }

    public class DocuSignManager : IDocuSignManager
    {
        public DocuSignApiConfiguration SetUp(AuthorizationTokenDO authTokenDO)
        {
            string baseUrl = string.Empty;
            string integratorKey = string.Empty;

            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);
            //create configuration for future api calls
            if (docuSignAuthDTO.IsDemoAccount)
            {
                integratorKey = CloudConfigurationManager.GetSetting("DocuSignIntegratorKey_DEMO");
                baseUrl = CloudConfigurationManager.GetSetting("environment_DEMO") + "restapi/";
            }
            else
            {
                integratorKey = CloudConfigurationManager.GetSetting("DocuSignIntegratorKey");
                baseUrl = docuSignAuthDTO.Endpoint.Replace("v2/accounts/" + docuSignAuthDTO.AccountId.ToString(), "");
            }
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

        public List<FieldDTO> GetTemplatesList(DocuSignApiConfiguration conf)
        {
            try
            {
                var tmpApi = new TemplatesApi(conf.Configuration);
                var result = tmpApi.ListTemplates(conf.AccountId);
                if (result.EnvelopeTemplates != null && result.EnvelopeTemplates.Count > 0)
                    return result.EnvelopeTemplates.Where(a => !string.IsNullOrEmpty(a.Name))
                        .Select(a => new FieldDTO(a.Name, a.TemplateId) { Availability = AvailabilityType.Configuration }).ToList();
                else
                    return new List<FieldDTO>();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public JObject DownloadDocuSignTemplate(DocuSignApiConfiguration config, string selectedDocusignTemplateId)
        {
            // we probably need to make multiple calls to api to collect all template info, i.e. recipients, tabs etc.
            //return Mapper.Map<DocuSignTemplateDTO>(jObjTemplate);

            throw new NotImplementedException();
        }

        public IEnumerable<FieldDTO> GetEnvelopeRecipientsAndTabs(DocuSignApiConfiguration conf, string envelopeId)
        {
            var envApi = new EnvelopesApi(conf.Configuration);
            return GetRecipientsAndTabs(conf, envApi, envelopeId);
        }

        public IEnumerable<FieldDTO> GetTemplateRecipientsAndTabs(DocuSignApiConfiguration conf, string templateId)
        {
            var tmpApi = new TemplatesApi(conf.Configuration);
            return GetRecipientsAndTabs(conf, tmpApi, templateId);
        }

        #region Send DocuSign Envelope methods

        //this is purely for Send_DocuSign_Envelope activity
        public Tuple<IEnumerable<FieldDTO>, IEnumerable<DocuSignTabDTO>> GetTemplateRecipientsTabsAndDocuSignTabs(DocuSignApiConfiguration conf, string templateId)
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

        public void SendAnEnvelopeFromTemplate(DocuSignApiConfiguration loginInfo, List<FieldDTO> rolesList, List<FieldDTO> fieldList, string curTemplateId, StandardFileDescriptionCM fileHandler = null)
        {
            EnvelopesApi envelopesApi = new EnvelopesApi(loginInfo.Configuration);
            TemplatesApi templatesApi = new TemplatesApi(loginInfo.Configuration);
            bool override_document = fileHandler != null;
            Recipients recipients = null;
            EnvelopeSummary envelopeSummary = null;

            //creatig an envelope definiton
            EnvelopeDefinition envDef = new EnvelopeDefinition();
            envDef.EmailSubject = "Test message from Fr8";
            envDef.Status = "created";


            var templateRecepients = templatesApi.ListRecipients(loginInfo.AccountId, curTemplateId);

            //adding file or applying template
            if (override_document)
            {
                //if we override document - we don't create an envelope yet 
                //we create it with recipients once we've processed recipient values and tabs
                envDef.Documents = new List<Document>() { new Document()
                { DocumentBase64 = fileHandler.TextRepresentation, FileExtension = fileHandler.Filetype,
                    DocumentId = "1", Name = fileHandler.Filename ?? Path.GetFileName(fileHandler.DirectUrl) ?? "document" } };
                recipients = templateRecepients;
            }
            else
            {
                //creating envelope
                envDef.TemplateId = curTemplateId;
                envelopeSummary = envelopesApi.CreateEnvelope(loginInfo.AccountId, envDef);
                //requesting list of recipients since their Ids might be different from the one we got from tempates
                recipients = envelopesApi.ListRecipients(loginInfo.AccountId, envelopeSummary.EnvelopeId);
            }

            //updating recipients
            foreach (var recepient in recipients.Signers)
            {
                var corresponding_template_recipient = templateRecepients.Signers.Where(a => a.RoutingOrder == recepient.RoutingOrder).FirstOrDefault();
                var related_fields = rolesList.Where(a => a.Tags.Contains("recipientId:" + corresponding_template_recipient.RecipientId));
                string new_email = related_fields.Where(a => a.Key.Contains("role email")).FirstOrDefault().Value;
                string new_name = related_fields.Where(a => a.Key.Contains("role name")).FirstOrDefault().Value;
                recepient.Name = string.IsNullOrEmpty(new_name) ? recepient.Name : new_name;
                recepient.Email = string.IsNullOrEmpty(new_email) ? recepient.Email : new_email;

                if (!recepient.Email.IsValidEmailAddress())
                {
                    throw new ApplicationException($"'{recepient.Email}' is not a valid email address");
                }

                //updating tabs
                var tabs = override_document ? templatesApi.ListTabs(loginInfo.AccountId, curTemplateId, corresponding_template_recipient.RecipientId, new Tabs()) : envelopesApi.ListTabs(loginInfo.AccountId, envelopeSummary.EnvelopeId, recepient.RecipientId);

                JObject jobj = DocuSignTab.ApplyValuesToTabs(fieldList, corresponding_template_recipient, tabs);
                recepient.Tabs = jobj.ToObject<Tabs>();
            }

            if (override_document)
            {
                //deep copy to exclude tabs
                var recps_deep_copy = JsonConvert.DeserializeObject<Recipients>(JsonConvert.SerializeObject(recipients));
                recps_deep_copy.Signers.ForEach(a => a.Tabs = null);
                envDef.Recipients = recps_deep_copy;
                //creating envlope
                envelopeSummary = envelopesApi.CreateEnvelope(loginInfo.AccountId, envDef);
            }
            else
            {
                envelopesApi.UpdateRecipients(loginInfo.AccountId, envelopeSummary.EnvelopeId, recipients);
            }

            foreach (var recepient in recipients.Signers)
            {
                if (override_document)
                {
                    JObject jobj = JObject.Parse(recepient.Tabs.ToJson());
                    foreach (var item in jobj.Properties())
                    {
                        foreach (var tab in (JToken)item.Value)
                        {
                            tab["documentId"] = "1";
                        }
                    }
                    var tabs = jobj.ToObject<Tabs>();
                    envelopesApi.CreateTabs(loginInfo.AccountId, envelopeSummary.EnvelopeId, recepient.RecipientId, tabs);
                }
                else
                    envelopesApi.UpdateTabs(loginInfo.AccountId, envelopeSummary.EnvelopeId, recepient.RecipientId, recepient.Tabs);
            }

            // sending an envelope
            envelopesApi.Update(loginInfo.AccountId, envelopeSummary.EnvelopeId, new Envelope() { Status = "sent" });
        }

        #endregion

        #region private methods

        private static IEnumerable<FieldDTO> GetRecipientsAndTabs(DocuSignApiConfiguration conf, object api, string id)
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

        private static Recipients GetRecipients(DocuSignApiConfiguration conf, object api, string id)
        {
            var templatesApi = api as TemplatesApi;
            if (templatesApi != null)
            {
                return templatesApi.ListRecipients(conf.AccountId, id) as Recipients;
            }
            var envelopesApi = api as EnvelopesApi;
            if (envelopesApi != null)
            {
                return envelopesApi.ListRecipients(conf.AccountId, id) as Recipients;
            }
            throw new NotSupportedException($"The api of '{api.GetType()}' is not supported");
        }

        private static IEnumerable<FieldDTO> GetTabs(DocuSignApiConfiguration conf, object api, string id, Signer recipient)
        {
            var envelopesApi = api as EnvelopesApi;
            var templatesApi = api as TemplatesApi;
            var docutabs = envelopesApi != null 
                            ? envelopesApi.ListTabs(conf.AccountId, id, recipient.RecipientId)
                            : templatesApi.ListTabs(conf.AccountId, id, recipient.RecipientId, new Tabs());

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
