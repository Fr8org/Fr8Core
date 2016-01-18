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

namespace terminalDocuSign.Actions
{
    public class BaseDocuSignAction : BaseTerminalAction
    {
        protected List<FieldDTO> CreateDocuSignEventFields()
        {
            return new List<FieldDTO>(){
                new FieldDTO("RecipientEmail") {Tags = "EmailAddress" },
                new FieldDTO("DocumentName"),
                new FieldDTO("TemplateName"),
                new FieldDTO("Status"),
                new FieldDTO("CreateDate") {Tags = "Date" },
                new FieldDTO("SentDate") {Tags = "Date" },
                new FieldDTO("DeliveredDate") {Tags = "Date" },
                new FieldDTO("CompletedDate") {Tags = "Date" },
                new FieldDTO("HolderEmail") {Tags = "EmailAddress" },
                new FieldDTO("Subject"),
                new FieldDTO("EnvelopeId"),
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

        public virtual async System.Threading.Tasks.Task<Data.Entities.ActionDO> Activate(Data.Entities.ActionDO curActionDO, Data.Entities.AuthorizationTokenDO authTokenDO)
        {
            //create DocuSign account if there is no existing connect profile
            DocuSignAccount.CreateOrUpdateDefaultDocuSignConnectConfiguration(null);

            return await Task.FromResult<ActionDO>(curActionDO);
        }
    }
}