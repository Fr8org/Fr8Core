using Data.Entities;
using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Data.Validations;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services.NewApi;
using System.IO;
using System.Text.RegularExpressions;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Errors;
using Fr8.TerminalBase.Models;
using terminalDocuSign.Infrastructure;

namespace terminalDocuSign.Services.New_Api
{
    public class DocuSignApiConfiguration
    {
        public string AccountId;
        public Configuration Configuration;
    }

    public class DocuSignManager : IDocuSignManager
    {
        public const string DocusignTerminalName = "terminalDocuSign";
        private static readonly string[] DefaultControlNames = new[] { "Text", "Checkbox", "Check Box", "Radio Group", "List", "Drop Down", "Note", "Number", "Data Field" };
        const string DefaultTemplateNameRegex = @"\s*\d+$";

        public DocuSignApiConfiguration SetUp(string authToken)
        {
            return SetUp(JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authToken));
        }

        public DocuSignApiConfiguration SetUp(AuthorizationToken authToken)
        {
            return SetUp(JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authToken.Token));
        }

        public DocuSignApiConfiguration SetUp(DocuSignAuthTokenDTO docuSignAuthDTO)
        {
            string baseUrl = string.Empty;
            string integratorKey = string.Empty;
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
                try
                {
                    LoginInformation loginInfo = authApi.Login();
                    result.AccountId = loginInfo.LoginAccounts[0].AccountId; //it seems that althought one DocuSign account can have multiple users - only one is returned, the one that oAuth token was created for
                }
                catch (Exception ex)
                {
                    throw new AuthorizationTokenExpiredOrInvalidException();
                }
            }

            return result;
        }


        public DocuSignEnvelopeCM_v2 GetEnvelope(DocuSignApiConfiguration config, string envelopeId)
        {
            DocuSignEnvelopeCM_v2 envelope;
            EnvelopesApi api = new EnvelopesApi(config.Configuration);
            //Templates
            var templates = api.ListTemplates(config.AccountId, envelopeId);
            var recipients = api.ListRecipients(config.AccountId, envelopeId);

            var filled_envelope = DocuSignEventParser.ParseAPIresponsesIntoCM(out envelope, templates, recipients);

            var envelopestatus = api.GetEnvelope(config.AccountId, envelopeId);
            filled_envelope.CreateDate = DateTime.Parse(envelopestatus.CreatedDateTime);
            filled_envelope.SentDate = DateTime.Parse(envelopestatus.SentDateTime);
            filled_envelope.StatusChangedDateTime = DateTime.Parse(envelopestatus.StatusChangedDateTime);
            envelope.Subject = envelopestatus.EmailSubject;
            envelope.EnvelopeId = envelopestatus.EnvelopeId;
            envelope.Status = envelopestatus.Status;

            return filled_envelope;
        }

        public List<KeyValueDTO> GetTemplatesList(DocuSignApiConfiguration conf)
        {
            var tmpApi = new TemplatesApi(conf.Configuration);
            var result = tmpApi.ListTemplates(conf.AccountId);
            if (result.EnvelopeTemplates != null && result.EnvelopeTemplates.Count > 0)
            {
                return result.EnvelopeTemplates.Where(a => !string.IsNullOrEmpty(a.Name))
                    .Select(a => new KeyValueDTO(a.Name, a.TemplateId)).ToList();
            }

            return new List<KeyValueDTO>();
        }

        public JObject DownloadDocuSignTemplate(DocuSignApiConfiguration config, string selectedDocusignTemplateId)
        {
            var templatesApi = new TemplatesApi(config.Configuration);
            var template = templatesApi.Get(config.AccountId, selectedDocusignTemplateId);
            foreach (var doc in template.Documents)
            {
                var document = templatesApi.GetDocument(config.AccountId, selectedDocusignTemplateId, doc.DocumentId);
                var ms = new MemoryStream();
                document.CopyTo(ms);
                string base64 = Convert.ToBase64String(ms.ToArray());
                template.Documents.Where(a => a.DocumentId == doc.DocumentId).FirstOrDefault().DocumentBase64 = base64;
            }
            var result = JsonConvert.SerializeObject(template);
            return JObject.Parse(result);
        }

        public IEnumerable<KeyValueDTO> GetEnvelopeRecipientsAndTabs(DocuSignApiConfiguration conf, string envelopeId)
        {
            var envApi = new EnvelopesApi(conf.Configuration);
            return GetRecipientsAndTabs(conf, envApi, envelopeId);
        }

        public IEnumerable<KeyValueDTO> GetTemplateRecipientsAndTabs(DocuSignApiConfiguration conf, string templateId)
        {
            var tmpApi = new TemplatesApi(conf.Configuration);
            return GetRecipientsAndTabs(conf, tmpApi, templateId);
        }

        #region Send DocuSign Envelope methods

        //this is purely for Send_DocuSign_Envelope activity
        public Tuple<IEnumerable<KeyValueDTO>, IEnumerable<DocuSignTabDTO>> GetTemplateRecipientsTabsAndDocuSignTabs(DocuSignApiConfiguration conf, string templateId)
        {
            var tmpApi = new TemplatesApi(conf.Configuration);
            var recipientsAndTabs = new List<KeyValueDTO>();
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

            return new Tuple<IEnumerable<KeyValueDTO>, IEnumerable<DocuSignTabDTO>>(recipientsAndTabs, docuTabs);
        }

        public void SendAnEnvelopeFromTemplate(DocuSignApiConfiguration loginInfo, List<KeyValueDTO> rolesList, List<KeyValueDTO> fieldList, string curTemplateId, StandardFileDescriptionCM fileHandler = null)
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

            Debug.WriteLine($"sending an envelope from template {curTemplateId} to {loginInfo}");
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
                var correspondingTemplateRecipient = templateRecepients.Signers.FirstOrDefault(a => a.RoutingOrder == recepient.RoutingOrder);
                var relatedFields = rolesList.Where(a => a.Tags.Contains("recipientId:" + correspondingTemplateRecipient?.RecipientId)).ToArray();
                var newEmail = relatedFields.FirstOrDefault(a => a.Key.Contains(DocuSignConstants.DocuSignRoleEmail))?.Value;
                var newName = relatedFields.FirstOrDefault(a => a.Key.Contains(DocuSignConstants.DocuSignRoleName))?.Value;
                recepient.Name = string.IsNullOrEmpty(newName) ? recepient.Name : newName;
                recepient.Email = string.IsNullOrEmpty(newEmail) ? recepient.Email : newEmail;

                if (!recepient.Email.IsValidEmailAddress())
                {
                    throw new ApplicationException($"'{recepient.Email}' is not a valid email address");
                }

                //updating tabs
                var tabs = override_document ? templatesApi.ListTabs(loginInfo.AccountId, curTemplateId, correspondingTemplateRecipient.RecipientId, new Tabs()) : envelopesApi.ListTabs(loginInfo.AccountId, envelopeSummary.EnvelopeId, recepient.RecipientId);

                JObject jobj = DocuSignTab.ApplyValuesToTabs(fieldList, correspondingTemplateRecipient, tabs);
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
                    if (typeof(Tabs).GetProperties()
                          .Select(prop => prop.GetValue(recepient.Tabs, null))
                          .Any(val => val != null))
                    envelopesApi.UpdateTabs(loginInfo.AccountId, envelopeSummary.EnvelopeId, recepient.RecipientId, recepient.Tabs);
            }

            // sending an envelope
            envelopesApi.Update(loginInfo.AccountId, envelopeSummary.EnvelopeId, new Envelope() { Status = "sent" });
        }

        public bool DocuSignTemplateDefaultNames(IEnumerable<DocuSignTabDTO> templateDefinedFields)
        {
            //filter out default names that start with the following strings: signature, initial, date signed

            //2) evalute the remaining fields and return true if at least 80 % of the fields match a default name pattern.This consists of:
            //a) a word from this list(Text, Checkbox, Radio Group, Drop Down, Name)
            //b) followed by a space
            //c) followed by an integer
            var result = false;
            var defaultTemplateNamesCount = 0;
            var totalTemplateNamesCount = 0;
            foreach (var item in templateDefinedFields)
            {
                totalTemplateNamesCount++;
                foreach (var x in DefaultControlNames)
                {
                    if (!item.Name.StartsWith(x)) continue;

                    int index = item.Name.IndexOf($"({item.RoleName})", StringComparison.Ordinal);
                    string cleanTemplateFieldName = (index < 0) ? item.Name : item.Name.Remove(index, item.RoleName.Length + 2);

                    var res = Regex.Match(cleanTemplateFieldName, DefaultTemplateNameRegex).Value;

                    var number = 0;
                    if (int.TryParse(res, out number))
                    {
                        defaultTemplateNamesCount++;
                    }
                }
            }

            var percentOfTemplateNames = ((double)defaultTemplateNamesCount / (double)totalTemplateNamesCount * 100);
            return percentOfTemplateNames >= 80;
        }


        #endregion

        #region private methods

        private static IEnumerable<KeyValueDTO> GetRecipientsAndTabs(DocuSignApiConfiguration conf, object api, string id)
        {
            try
            {
                var result = new List<KeyValueDTO>();
                var recipients = GetRecipients(conf, api, id);
                result.AddRange(MapRecipientsToFieldDTO(recipients));
                foreach (var recipient in recipients.Signers)
                {
                    result.AddRange(GetTabs(conf, api, id, recipient));
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new AuthorizationTokenExpiredOrInvalidException();
            }
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

        private static IEnumerable<KeyValueDTO> GetTabs(DocuSignApiConfiguration conf, object api, string id, Signer recipient)
        {
            var envelopesApi = api as EnvelopesApi;
            var templatesApi = api as TemplatesApi;
            var docutabs = envelopesApi != null
                            ? envelopesApi.ListTabs(conf.AccountId, id, recipient.RecipientId)
                            : templatesApi.ListTabs(conf.AccountId, id, recipient.RecipientId, new Tabs());

            return (DocuSignTab.GetEnvelopeTabsPerSigner(JObject.Parse(docutabs.ToJson()), recipient.RoleName));
        }

        private static IEnumerable<KeyValueDTO> MapRecipientsToFieldDTO(Recipients recipients)
        {
            var result = new List<KeyValueDTO>();
            if (recipients.Signers != null)
                recipients.Signers.ForEach(
                    a =>
                    {
                        //use RoleName. If unavailable use a Name. If unavaible use email
                        result.Add(new KeyValueDTO((a.RoleName ?? a.Name ?? a.Email) + " role name", a.Name) { Tags = "DocuSigner, recipientId:" + a.RecipientId });
                        result.Add(new KeyValueDTO((a.RoleName ?? a.Name ?? a.Email) + " role email", a.Email) { Tags = "DocuSigner, recipientId:" + a.RecipientId });
                    });
            return result;
        }

        #endregion

    }
}
