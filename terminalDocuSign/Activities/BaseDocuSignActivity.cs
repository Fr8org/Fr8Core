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

namespace terminalDocuSign.Actions
{
    public abstract class BaseDocuSignActivity : BaseTerminalActivity
    {
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

        protected string GetValueForKey(PayloadDTO curPayloadDTO, string curKey)
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

        public override async Task<ActivityDO> Activate(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            //create DocuSign account if there is no existing connect profile
            DocuSignAccount.CreateOrUpdateDefaultDocuSignConnectConfiguration(null);
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