using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using DocuSign.Integrations.Client;
using Newtonsoft.Json.Linq;
using Utilities;

namespace Data.Wrappers
{
    public interface IDocuSignTemplate
    {
        TemplateInfo Create(TemplateInfo submissionData);
        IEnumerable<TemplateInfo> GetByAccount(DockyardAccountDO curDockyardAccount);
        bool CreateTemplate(List<byte[]> fileBytesList, List<string> fileNames, string templateName);
        JObject GetTemplate(string templateId);
        byte[] GetTemplatePreview(string templateId);
        List<TemplateInfo> GetTemplates();
        byte[] GetCompletedDocument(string envelopeId, bool includeCertificate);
        EnvelopeDocuments GetEnvelopeDocumentInfo(string envelopeId);
        bool AddTemplates(List<string> templateIds);
        bool AddTemplates(DocumentTemplates templates);
        EnvelopeTemplates GetEnvelopeMatchingTemplates();
        bool GetCustomFields();
        bool AddTabs(TabCollection tabs);
        bool AddEmailInformation(string subject, string blurb);
        bool SendReminder();
        bool AddRecipients(Recipients recipients);
        bool UpdateRecipients(Recipients recipients, bool resendEnvelope);
        bool GetRecipients();
        bool GetRecipients(bool includeTabs, bool includeExtended);
        DateTime GetStatus(string envelopeId);
        List<string> GetDocNames(string envelopeID);
        List<string> GetDocIds(string envelopeID);
        List<EnvelopeDocument> GetDocuments();
        bool UpdateStatus(string voidedReason);
        IEnumerable<string> GetRecipientNames();
        IEnumerable<JToken> GetFirstRecipients();
        bool Create(string path);
        bool Create(byte[] fileBytes, string fileName);
        bool Create(Stream fileStream, string fileName);
        bool Create();
        bool Create(List<byte[]> fileBytesList, List<string> fileNames);
        bool GetRecipientView();
        bool GetRecipientView(string returnUrl, bool signAndReturn, string authMethod);
        string GetEmbeddedSignerView(string returnUrl, DocuSign.Integrations.Client.Signer signer, string authMethod);
        bool GetSenderView(string returnUrl);
        bool AddCustomFields(Dictionary<string, object> customFields);
        bool AddDocument(byte[] fileBytes, string fileName, int index);
        bool AddDocument(List<byte[]> fileBytesList, List<string> fileNames, int index);
        bool RemoveDocument(int docId);
        bool RemoveDocument(List<string> docList);
        AccountEnvelopes GetAccountsEnvelopes(DateTime fromDate);
        AccountEnvelopes GetDraftEnvelopes(DateTime fromDate);
        int GetSearchFolderCount(string folderName, DateTime fromDate);
        IEnumerable<DocuSign.Integrations.Client.Signer> AllRecipients { get; }
        Account Login { get; set; }
        string EnvelopeId { get; set; }
        string Url { get; }
        string SenderViewUrl { get; }
        Notification Notification { get; set; }
        WebProxy Proxy { get; set; }
        CustomFields CustomFields { get; set; }
        eventNotification Events { get; set; }
        Recipients Recipients { get; set; }
        TemplateRole[] TemplateRoles { get; set; }
        CompositeTemplate[] CompositeTemplates { get; set; }
        Recipients CarbonCopies { get; set; }
        string EmailBlurb { get; set; }
        string EmailSubject { get; set; }
        string Status { get; set; }
        DateTime Created { get; set; }
        Error RestError { get; }
        string MimeType { get; set; }
        string RestTrace { get; }
        string TemplateId { get; set; }
    }

    public class DocuSignTemplate : DocuSign.Integrations.Client.Template, IDocuSignTemplate
    {
         

        public DocuSignTemplate()
        {
             
        }

        public TemplateInfo Create(TemplateInfo submissionData)
        {
            //replace with a real implementation that calls POST /accounts/{accountId}/templates
            //for now, just adding arbitrary id to help with testing
            submissionData.Id = "234";
            return submissionData; 
        }

        public IEnumerable<TemplateInfo> GetByAccount(DockyardAccountDO curDockyardAccount)
        {
            Template curTemplate = new DocuSignTemplate();
            //curTemplate.Login = 
            return new List<TemplateInfo>();
        }

    }

}
