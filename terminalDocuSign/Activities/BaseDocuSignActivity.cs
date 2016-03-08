using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Data.Crates;
using TerminalBase.BaseClasses;
using terminalDocuSign.Infrastructure;
using Hub.Managers;
using Data.States;
using Newtonsoft.Json;
using terminalDocuSign.Services.New_Api;
using StructureMap;
using Data.Control;

namespace terminalDocuSign.Actions
{
    public class BaseDocuSignActivity : BaseTerminalActivity
    {

        protected ICrateManager Crate;

        public BaseDocuSignActivity()
        {
            Crate = ObjectFactory.GetInstance<ICrateManager>();
        }

        protected List<FieldDTO> CreateDocuSignEventFields()
        {
            return new List<FieldDTO>(){
                new FieldDTO("RecipientEmail", AvailabilityType.RunTime) {Tags = "EmailAddress" },
                new FieldDTO("RecipientUserName", AvailabilityType.RunTime) {Tags = "UserName" },
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

        public void AddOrUpdateUserDefinedFields(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO, IUpdatableCrateStorage updater, string templateId, string envelopeId = null)
        {
            updater.RemoveByLabel("DocuSignTemplateUserDefinedFields");
            if (!String.IsNullOrEmpty(templateId))
            {
                var conf = DocuSignService.SetUp(authTokenDO);
                var userDefinedFields = DocuSignService.GetTemplateRecipientsAndTabs(conf, templateId);
                updater.Add(Crate.CreateDesignTimeFieldsCrate("DocuSignTemplateUserDefinedFields", AvailabilityType.RunTime, userDefinedFields.ToArray()));
            }
        }

        public StandardPayloadDataCM CreateActivityPayload(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO, string curEnvelopeId)
        {
            var conf = DocuSignService.SetUp(authTokenDO);
            var payload = DocuSignService.GetEnvelopeRecipientsAndTabs(conf, curEnvelopeId);
            return new StandardPayloadDataCM(payload.ToArray());
        }

        public void FillDocuSignTemplateSource(Crate configurationCrate, string controlName, AuthorizationTokenDO authToken)
        {
            var configurationControl = configurationCrate.Get<StandardConfigurationControlsCM>();
            var control = configurationControl.FindByNameNested<DropDownList>(controlName);
            if (control != null)
            {
                var conf = DocuSignService.SetUp(authToken);
                var templates = DocuSignService.GetTemplatesList(conf);
                control.ListItems = templates.Select(x => new ListItem() { Key = x.Key, Value = x.Value }).ToList();
            }
        }

        public virtual async System.Threading.Tasks.Task<Data.Entities.ActivityDO> Activate(Data.Entities.ActivityDO curActivityDO, Data.Entities.AuthorizationTokenDO authTokenDO)
        {
            //create DocuSign account if there is no existing connect profile
            DocuSignAccount.CreateOrUpdateDefaultDocuSignConnectConfiguration(null);

            return await Task.FromResult<ActivityDO>(curActivityDO);
        }
    }
}