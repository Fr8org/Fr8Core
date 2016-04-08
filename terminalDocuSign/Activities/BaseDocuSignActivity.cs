using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
using Data.Control;
using Data.Crates;
using TerminalBase.BaseClasses;
using terminalDocuSign.Infrastructure;
using Hub.Managers;
using Data.States;
using terminalDocuSign.Services.New_Api;
using StructureMap;

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

        protected List<FieldDTO> CreateDocuSignEventFields()
        {
            return new List<FieldDTO>{
                new FieldDTO("RecipientEmail", AvailabilityType.RunTime) { Tags = "EmailAddress" },
                new FieldDTO("RecipientUserName", AvailabilityType.RunTime) { Tags = "UserName" },
                new FieldDTO("DocumentName", AvailabilityType.RunTime),
                new FieldDTO("TemplateName", AvailabilityType.RunTime),
                new FieldDTO("Status", AvailabilityType.RunTime),
                new FieldDTO("CreateDate") { Tags = "Date" },
                new FieldDTO("SentDate", AvailabilityType.RunTime) { Tags = "Date" },
                new FieldDTO("DeliveredDate", AvailabilityType.RunTime) { Tags = "Date" },
                new FieldDTO("CompletedDate", AvailabilityType.RunTime) { Tags = "Date" },
                new FieldDTO("HolderEmail", AvailabilityType.RunTime) { Tags = "EmailAddress" },
                new FieldDTO("Subject", AvailabilityType.RunTime),
                new FieldDTO("EnvelopeId", AvailabilityType.RunTime),
            };
        }

        protected string GetValueForEventKey(PayloadDTO curPayloadDTO, string curKey)
        {
            var eventReportMS = CrateManager.GetStorage(curPayloadDTO).CrateContentsOfType<EventReportCM>().FirstOrDefault();

            if (eventReportMS == null)
            {
                return null;
            }

            var crate = eventReportMS.EventPayload.CratesOfType<StandardPayloadDataCM>().First();

            if (crate == null)
            {
                return null;
            }

            var fields = crate.Content.AllValues().ToArray();
            if (fields == null || fields.Length == 0) return null;

            var envelopeIdField = fields.SingleOrDefault(f => f.Key == curKey);
            if (envelopeIdField == null) return null;

            return envelopeIdField.Value;
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

        public StandardPayloadDataCM CreateActivityPayload(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO, string curEnvelopeId)
        {
            var conf = DocuSignManager.SetUp(authTokenDO);
            var payload = DocuSignManager.GetEnvelopeRecipientsAndTabs(conf, curEnvelopeId);
            return new StandardPayloadDataCM(payload.ToArray());
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
            return await RunInternal(activityDO, containerId, authTokenDO);
        }

        protected abstract string ActivityUserFriendlyName { get; }

        protected internal abstract Task<PayloadDTO> RunInternal(ActivityDO activityDO, Guid containerId, AuthorizationTokenDO authTokenDO);
    }
}