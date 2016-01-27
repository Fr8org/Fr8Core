using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TerminalBase.BaseClasses;
using terminalDocuSign.Infrastructure;
using Hub.Managers;
using Data.States;

namespace terminalDocuSign.Actions
{
    public class BaseDocuSignAction : BaseTerminalActivity
    {
        protected List<FieldDTO> CreateDocuSignEventFields()
        {
            return new List<FieldDTO>(){
                new FieldDTO("RecipientEmail", AvailabilityType.RunTime) {Tags = "EmailAddress" },
                new FieldDTO("DocumentName", AvailabilityType.RunTime),
                new FieldDTO("TemplateName", AvailabilityType.RunTime),
                new FieldDTO("Status", AvailabilityType.RunTime),
                new FieldDTO("CreateDate") {Tags = "Date" },
                new FieldDTO("SentDate", AvailabilityType.RunTime) {Tags = "Date" },
                new FieldDTO("DeliveredDate", AvailabilityType.RunTime) {Tags = "Date" },
                new FieldDTO("CompletedDate", AvailabilityType.RunTime) {Tags = "Date" },
                new FieldDTO("HolderEmail", AvailabilityType.RunTime) {Tags = "EmailAddress" },
                new FieldDTO("Subject", AvailabilityType.RunTime),
                new FieldDTO("EnvelopeId", AvailabilityType.RunTime),
            };
        }

        protected string GetValueForKey(PayloadDTO curPayloadDTO, string curKey)
        {
            var eventReportMS = Crate.GetStorage(curPayloadDTO).CrateContentsOfType<EventReportCM>().FirstOrDefault();

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

        public virtual async System.Threading.Tasks.Task<Data.Entities.ActivityDO> Activate(Data.Entities.ActivityDO curActivityDO, Data.Entities.AuthorizationTokenDO authTokenDO)
        {
            //create DocuSign account if there is no existing connect profile
            DocuSignAccount.CreateOrUpdateDefaultDocuSignConnectConfiguration(null);

            return await Task.FromResult<ActivityDO>(curActivityDO);
        }
    }
}