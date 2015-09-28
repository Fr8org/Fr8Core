using Data.Entities;
using PluginBase.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Interfaces.DataTransferObjects;
using PluginBase.BaseClasses;
using Newtonsoft.Json;
using Core.Interfaces;
using StructureMap;
using System.Web.Http;
using System.Web.Http.Results;
using PluginBase;
using Data.Interfaces;
using Data.Interfaces.ManifestSchemas;

namespace pluginDocuSign.Actions
{
    public class Extract_From_DocuSign_Envelope_v1 : BasePluginAction
    {
        ICrate _crate = ObjectFactory.GetInstance<ICrate>();
        IAction _action = ObjectFactory.GetInstance<IAction>();
        IEnvelope _envelope = ObjectFactory.GetInstance<IEnvelope>();

        public ActionDTO Configure(ActionDTO curActionDTO)
        {
            //TODO: The coniguration feature for Docu Sign is not yet defined. The configuration evaluation needs to be implemented.
            return ProcessConfigurationRequest(curActionDTO, actionDo => ConfigurationRequestType.Initial); // will be changed to complete the config feature for docu sign
        }

        public void Activate(ActionDTO curActionDTO)
        {
            return; // Will be changed when implementation is plumbed in.
        }

        public void Deactivate(ActionDTO curActionDTO)
        {
            return; // Will be changed when implementation is plumbed in.
        }

        public void Execute(ActionDataPackageDTO curActionDataPackageDTO)
        {
            //Get envlopeId
            string envelopeId = GetEnvelopeId(curActionDataPackageDTO.PayloadDTO);
            if (envelopeId == null)
            {
                throw new PluginCodedException(PluginErrorCode.PAYLOAD_DATA_MISSING, "EnvelopeId");
            }

            var payload = CreateActionPayload(curActionDataPackageDTO.ActionDTO, envelopeId);
            var cratesList = new List<CrateDTO>()
            {
                _crate.Create("DocuSign Envelope Data",
                JsonConvert.SerializeObject(payload), CrateManifests.STANDARD_PAYLOAD_MANIFEST_NAME, CrateManifests.STANDARD_PAYLOAD_MANIFEST_ID)
            };
            curActionDataPackageDTO.PayloadDTO.UpdateCrateStorageDTO(cratesList);     
        }

        public IList<FieldDTO> CreateActionPayload(ActionDTO curActionDO, string curEnvelopeId)
        {
            var curEnvelopeData = _envelope.GetEnvelopeData(curEnvelopeId);
            var fields = GetFields(curActionDO);

            if (fields.Count == 0)
            {
                throw new InvalidOperationException("Field mappings are empty on ActionDO with id " + curActionDO.Id);
            }
            return _envelope.ExtractPayload(fields, curEnvelopeId, curEnvelopeData);
        }

        private List<FieldDTO> GetFields(ActionDTO curActionDO)
        {
            var activityDO = AutoMapper.Mapper.Map<ActionDO>(curActionDO) as ActivityDO;
            var crates = GetCratesByDirection(activityDO, CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME, GetCrateDirection.Downstream);

            if (crates.Count() == 0) return null;

            // Merge fields of multiple crates
            var fieldsList = MergeContentFields(crates).Fields;

            if (fieldsList == null || fieldsList.Count == 0) return null;

            return fieldsList;
        }

        private string GetEnvelopeId(PayloadDTO curPayloadDTO)
        {
            var crate = curPayloadDTO.CrateStorageDTO().CrateDTO.SingleOrDefault();
            if (crate == null) return null; //TODO: log it

            var standardPayloadMS = JsonConvert.DeserializeObject<EventReportMS>(crate.Contents);
            var payload = standardPayloadMS.EventPayload.SingleOrDefault();
            if (payload == null)
            {
                return null;
            }

            var fields = JsonConvert.DeserializeObject<List<FieldDTO>>(payload.Contents);
            if (fields == null || fields.Count == 0)
            {
                return null; // TODO: log it
            }
            var envelopeIdField = fields.SingleOrDefault(f => f.Key == "EnvelopeId");
            if (envelopeIdField == null || string.IsNullOrEmpty(envelopeIdField.Value))
            {
                return null; // TODO: log it
            }
            return envelopeIdField.Value;
        }

        protected override ActionDTO InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            // "[{ type: 'textField', name: 'connection_string', required: true, value: '', fieldLabel: 'SQL Connection String' }]"
            var textBlock = new TextBlockFieldDTO()
            {
                FieldLabel = "Docu Sign Envelope",
                Value = "This Action doesn't require any configuration.",
                Type = "textBlockField",
                cssClass = "well well-lg"

            };

            var crateControls = PackControlsCrate(textBlock);

            curActionDTO.CrateStorage.CrateDTO.Add(crateControls);
            return curActionDTO;
        }
    }
}