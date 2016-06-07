using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using StructureMap;
using terminalDocuSign.Services.New_Api;
using TerminalBase.BaseClasses;
using TerminalBase.Errors;
using TerminalBase.Infrastructure;

namespace terminalDocuSign.Activities
{
    public abstract class BaseDocuSignActivity : BaseTerminalActivity
    {
        protected IDocuSignManager DocuSignManager;


        protected BaseDocuSignActivity(ICrateManager crateManager, IDocuSignManager docuSignManager)
            : base(crateManager)
        {
            DocuSignManager = docuSignManager;
        }

        public override async Task Initialize()
        {
            await Configure(InitializeDS);
        }

        public override async Task FollowUp()
        {
            await Configure(FollowUpDS);
        }

        public async Task Configure(Func<Task> configFunc)
        {
            try
            {
                await configFunc();
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(ex.Message) && ex.Message.Contains("AUTHORIZATION_INVALID_TOKEN"))
                {
                    AddAuthenticationCrate(true);
                    return;
                }

                throw;
            }
        }

        protected List<FieldDTO> CreateDocuSignEventFields(DocuSignEnvelopeCM_v2 envelope, string label = null)
        {
            string curRecipientEmail = "";
            string curRecipientUserName = "";

            if (envelope != null)
            {
                var current_recipient = envelope.GetCurrentRecipient();
                curRecipientEmail = current_recipient.Email;
                curRecipientUserName = current_recipient.Name;
            }

            return new List<FieldDTO>{
                new FieldDTO("CurrentRecipientEmail", curRecipientEmail, AvailabilityType.RunTime) { Tags = "EmailAddress",SourceCrateLabel = label },
                new FieldDTO("CurrentRecipientUserName", curRecipientUserName, AvailabilityType.RunTime) { Tags = "UserName", SourceCrateLabel = label },
                new FieldDTO("Status", envelope?.Status,  AvailabilityType.RunTime) { SourceCrateLabel = label},
                new FieldDTO("CreateDate",  envelope?.CreateDate?.ToString()) { Tags = "Date",SourceCrateLabel = label },
                new FieldDTO("SentDate", envelope?.SentDate?.ToString(), AvailabilityType.RunTime) { Tags = "Date", SourceCrateLabel = label },
                new FieldDTO("Subject", envelope?.Subject, AvailabilityType.RunTime) { SourceCrateLabel = label},
                new FieldDTO("EnvelopeId", envelope?.EnvelopeId, AvailabilityType.RunTime) { SourceCrateLabel = label},
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
            return DocuSignManager.GetTemplateRecipientsAndTabs(conf, templateId);
        }

        public IEnumerable<FieldDTO> GetEnvelopeData(string templateId, string envelopeId = null)
        {
            if (String.IsNullOrEmpty(templateId))
            {
                throw new ArgumentNullException(nameof(templateId));
            }
            var conf = DocuSignManager.SetUp(AuthorizationToken);
            return DocuSignManager.GetEnvelopeRecipientsAndTabs(conf, templateId);
        }

        public void AddOrUpdateUserDefinedFields(string templateId, string envelopeId = null, List<FieldDTO> allFields = null)
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
                Storage.Add(CrateManager.CreateDesignTimeFieldsCrate("DocuSignTemplateUserDefinedFields", AvailabilityType.RunTime, userDefinedFields.ToArray()));
            }
        }

        public void FillDocuSignTemplateSource(Crate configurationCrate, string controlName)
        {
            var configurationControl = configurationCrate.Get<StandardConfigurationControlsCM>();
            var control = configurationControl.FindByNameNested<DropDownList>(controlName);
            if (control != null)
            {
                var conf = DocuSignManager.SetUp(AuthorizationToken);
                var templates = DocuSignManager.GetTemplatesList(conf);
                control.ListItems = templates.Select(x => new ListItem() { Key = x.Key, Value = x.Value }).ToList();
            }
            }

        public override async Task Run()
            {
            try
            {
                await RunDS();
                Success();
            }
            catch (AuthorizationTokenExpiredOrInvalidException ex)
            {
                RaiseInvalidTokenError(ex.Message);
            }
            catch (DocuSign.eSign.Client.ApiException ex)
            {
                if (ex.ErrorCode == 401)
                {
                    RaiseInvalidTokenError();
                }
                else
                {
                    throw;
                }
            }
        }

        protected abstract string ActivityUserFriendlyName { get; }

        protected abstract Task RunDS();
        protected abstract Task InitializeDS();
        protected abstract Task FollowUpDS();
    }
}