using System;
using System.Collections.Generic;
using System.Linq;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using terminalDocuSign.Services.New_Api;

namespace terminalDocuSign.Activities
{
    public abstract class BaseDocuSignActivity : ExplicitTerminalActivity
    {
        protected IDocuSignManager DocuSignManager;

        protected BaseDocuSignActivity(ICrateManager crateManager, IDocuSignManager docuSignManager)
            : base(crateManager)
        {
            DocuSignManager = docuSignManager;
        }


        protected List<KeyValueDTO> CreateDocuSignEventValues(DocuSignEnvelopeCM_v2 envelope)
        {
            string curRecipientEmail = "";
            string curRecipientUserName = "";

            if (envelope != null)
            {
                var current_recipient = envelope.GetCurrentRecipient();
                curRecipientEmail = current_recipient.Email;
                curRecipientUserName = current_recipient.Name;
            }

            return new List<KeyValueDTO>
            {
                new KeyValueDTO("CurrentRecipientEmail", curRecipientEmail) {Tags = "EmailAddress"},
                new KeyValueDTO("CurrentRecipientUserName", curRecipientUserName) {Tags = "UserName"},
                new KeyValueDTO("EnvelopeStatus", envelope?.Status),
                new KeyValueDTO("CreateDate", envelope?.CreateDate?.ToString()) {Tags = "Date"},
                new KeyValueDTO("SentDate", envelope?.SentDate?.ToString()) {Tags = "Date"},
                new KeyValueDTO("Subject", envelope?.Subject),
                new KeyValueDTO("EnvelopeId", envelope?.EnvelopeId),
            };
        }

        protected List<FieldDTO> CreateDocuSignEventFieldsDefinitions(string label = null)
        {
            return new List<FieldDTO>
            {
                new FieldDTO("CurrentRecipientEmail", AvailabilityType.RunTime) {Tags = "EmailAddress", SourceCrateLabel = label},
                new FieldDTO("CurrentRecipientUserName", AvailabilityType.RunTime) {Tags = "UserName", SourceCrateLabel = label},
                new FieldDTO("EnvelopeStatus", AvailabilityType.RunTime) {SourceCrateLabel = label},
                new FieldDTO("CreateDate") {Tags = "Date", SourceCrateLabel = label},
                new FieldDTO("SentDate", AvailabilityType.RunTime) {Tags = "Date", SourceCrateLabel = label},
                new FieldDTO("Subject", AvailabilityType.RunTime) {SourceCrateLabel = label},
                new FieldDTO("EnvelopeId", AvailabilityType.RunTime) {SourceCrateLabel = label},
            };
        }

        public static DropDownList CreateDocuSignTemplatePicker(bool addOnChangeEvent,
            string name = "Selected_DocuSign_Template",
            string label = "Select DocuSign Template")
        {
            var control = new DropDownList()
            {
                Label = label,
                Name = name,
                Required = true,
                Source = null
            };

            if (addOnChangeEvent)
            {
                control.Events = new List<ControlEvent>()
                {
                    new ControlEvent("onChange", "requestConfig")
                };
            }

            return control;
        }

        public IEnumerable<FieldDTO> GetTemplateUserDefinedFields(string templateId, string envelopeId = null)
        {
            if (String.IsNullOrEmpty(templateId))
            {
                throw new ArgumentNullException(nameof(templateId));
            }
            var conf = DocuSignManager.SetUp(AuthorizationToken);
            var result = DocuSignManager.GetTemplateRecipientsAndTabs(conf, templateId).Select(x => new FieldDTO(x.Key) { Tags = x.Tags });
            return result;
        }

        public IEnumerable<KeyValueDTO> GetEnvelopeData(string envelopeId)
        {
            if (String.IsNullOrEmpty(envelopeId))
            {
                throw new ArgumentNullException(nameof(envelopeId));
            }
            var conf = DocuSignManager.SetUp(AuthorizationToken);
            return DocuSignManager.GetEnvelopeRecipientsAndTabs(conf, envelopeId);
        }

        public void AddOrUpdateUserDefinedFields(string templateId, string envelopeId = null, List<KeyValueDTO> allFields = null)
        {
            Storage.RemoveByLabel("DocuSignTemplateUserDefinedFields");
            if (!String.IsNullOrEmpty(templateId))
            {
                var conf = DocuSignManager.SetUp(AuthorizationToken);
                var userDefinedFields = DocuSignManager.GetTemplateRecipientsAndTabs(conf, templateId);

                if (allFields != null)
                {
                    allFields.AddRange(userDefinedFields);
                }

                Storage.Add("DocuSignTemplateUserDefinedFields", new KeyValueListCM(userDefinedFields));
            }
        }

        public void FillDocuSignTemplateSource(string controlName)
        {
            var control = ConfigurationControls.FindByNameNested<DropDownList>(controlName);

            if (control != null)
            {
                var conf = DocuSignManager.SetUp(AuthorizationToken);
                var templates = DocuSignManager.GetTemplatesList(conf);
                control.ListItems = templates.Select(x => new ListItem() { Key = x.Key, Value = x.Value }).ToList();
            }
        }

        protected override bool IsInvalidTokenException(Exception ex)
        {
            var docusignApiException = ex as DocuSign.eSign.Client.ApiException;

            if (docusignApiException != null && docusignApiException.ErrorCode == 401)
            {
                return true;
            }

            return ex.Message.Contains("AUTHORIZATION_INVALID_TOKEN");
        }

        protected abstract string ActivityUserFriendlyName { get; }
    }
}