using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using StructureMap;
using terminalDocuSign.Services.New_Api;
using TerminalBase.BaseClasses;
using TerminalBase.Errors;

namespace terminalDocuSign.Actions
{
    public abstract class BaseDocuSignActivity : BaseTerminalActivity
    {
        protected IDocuSignManager DocuSignManager;

        protected ICrateManager Crate;

        public BaseDocuSignActivity()
        {
            Crate = ObjectFactory.GetInstance<ICrateManager>();
            DocuSignManager = ObjectFactory.GetInstance<IDocuSignManager>();
        }

        public override async Task<ActivityDO> Configure(ActivityDO activityDO, AuthorizationTokenDO authTokenDO)
        {
            try
            {
                return await ProcessConfigurationRequest(activityDO, ConfigurationEvaluator, authTokenDO);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(ex.Message) && ex.Message.Contains("AUTHORIZATION_INVALID_TOKEN"))
                {
                    AddAuthenticationCrate(activityDO, true);
                    return activityDO;
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
                new FieldDTO("CurrentRecipientEmail", curRecipientEmail, AvailabilityType.RunTime) { Tags = "EmailAddress",Label = label },
                new FieldDTO("CurrentRecipientUserName", curRecipientUserName, AvailabilityType.RunTime) { Tags = "UserName", Label = label },
                new FieldDTO("Status", envelope?.Status,  AvailabilityType.RunTime) { Label = label},
                new FieldDTO("CreateDate",  envelope?.CreateDate?.ToString()) { Tags = "Date",Label = label },
                new FieldDTO("SentDate", envelope?.SentDate?.ToString(), AvailabilityType.RunTime) { Tags = "Date", Label = label },
                new FieldDTO("Subject", envelope?.Subject, AvailabilityType.RunTime) { Label = label},
                new FieldDTO("EnvelopeId", envelope?.EnvelopeId, AvailabilityType.RunTime) { Label = label},
            };
        }

        public static DropDownList CreateDocuSignTemplatePicker(
            bool addOnChangeEvent,
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

        public IEnumerable<FieldDTO> GetTemplateUserDefinedFields(AuthorizationTokenDO authTokenDO, string templateId, string envelopeId = null)
        {
            if (String.IsNullOrEmpty(templateId))
            {
                throw new ArgumentNullException(nameof(templateId));
            }
            var conf = DocuSignManager.SetUp(authTokenDO);
            return DocuSignManager.GetTemplateRecipientsAndTabs(conf, templateId);
        }

        public IEnumerable<FieldDTO> GetEnvelopeData(AuthorizationTokenDO authTokenDO, string templateId, string envelopeId = null)
        {
            if (String.IsNullOrEmpty(templateId))
            {
                throw new ArgumentNullException(nameof(templateId));
            }
            var conf = DocuSignManager.SetUp(authTokenDO);
            return DocuSignManager.GetEnvelopeRecipientsAndTabs(conf, templateId);
        }

        public void AddOrUpdateUserDefinedFields(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO, IUpdatableCrateStorage updater, string templateId, string envelopeId = null, List<FieldDTO> allFields = null)
        {
            updater.RemoveByLabel("DocuSignTemplateUserDefinedFields");
            if (!String.IsNullOrEmpty(templateId))
            {
                var conf = DocuSignManager.SetUp(authTokenDO);
                var userDefinedFields = DocuSignManager.GetTemplateRecipientsAndTabs(conf, templateId);
                if (allFields != null)
                {
                    allFields.AddRange(userDefinedFields);
                }
                updater.Add(Crate.CreateDesignTimeFieldsCrate("DocuSignTemplateUserDefinedFields", AvailabilityType.RunTime, userDefinedFields.ToArray()));
            }
        }

        public void FillDocuSignTemplateSource(Crate configurationCrate, string controlName, AuthorizationTokenDO authToken)
        {
            var configurationControl = configurationCrate.Get<StandardConfigurationControlsCM>();
            var control = configurationControl.FindByNameNested<DropDownList>(controlName);
            if (control != null)
            {
                var conf = DocuSignManager.SetUp(authToken);
                var templates = DocuSignManager.GetTemplatesList(conf);
                control.ListItems = templates.Select(x => new ListItem() { Key = x.Key, Value = x.Value }).ToList();
            }
        }

        public virtual async System.Threading.Tasks.Task<Data.Entities.ActivityDO> Activate(Data.Entities.ActivityDO curActivityDO, Data.Entities.AuthorizationTokenDO authTokenDO)
        {
            return await base.Activate(curActivityDO, authTokenDO);
        }

        protected override async Task<ICrateStorage> ValidateActivity(ActivityDO curActivityDO)
        {
            var result = ValidateActivityInternal(curActivityDO);
            if (result == ValidationResult.Success)
            {
                return await Task.FromResult<ICrateStorage>(null);
            }
            return await Task.FromResult(new CrateStorage(Crate<FieldDescriptionsCM>.FromContent("Validation Errors",
                                                                                                 new FieldDescriptionsCM(new FieldDTO("Error Message", result.ErrorMessage)))));
        }

        protected internal virtual ValidationResult ValidateActivityInternal(ActivityDO curActivityDO)
        {
            return ValidationResult.Success;
        }

        public async Task<PayloadDTO> Run(ActivityDO activityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(activityDO, containerId);
            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(payloadCrates);
            }

            var result = ValidateActivityInternal(activityDO);
            if (result != ValidationResult.Success)
            {
                return Error(payloadCrates, $"Could not run {ActivityUserFriendlyName} because of the below issues:{Environment.NewLine}{result.ErrorMessage}", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }

            try
            {
                return await RunInternal(activityDO, containerId, authTokenDO);
            }
            catch (AuthorizationTokenExpiredOrInvalidException ex)
            {
                return InvalidTokenError(payloadCrates, ex.Message);
            }
            catch (DocuSign.eSign.Client.ApiException ex)
            {
                if (ex.ErrorCode == 401)
                {
                    return InvalidTokenError(payloadCrates);
                }
                else
                {
                    throw;
                }
            }
        }

        protected abstract string ActivityUserFriendlyName { get; }

        protected internal abstract Task<PayloadDTO> RunInternal(ActivityDO activityDO, Guid containerId, AuthorizationTokenDO authTokenDO);
    }
}