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
using System.Threading.Tasks;
using pluginDocuSign.Interfaces;

namespace pluginDocuSign.Actions
{
    public class Extract_From_DocuSign_Envelope_v1 : BasePluginAction
    {
		 IDocuSignEnvelope _docusignEnvelope = ObjectFactory.GetInstance<IDocuSignEnvelope>();

		 public Extract_From_DocuSign_Envelope_v1()
		 {
			 _docusignEnvelope = ObjectFactory.GetInstance<IDocuSignEnvelope>();
		 }

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

        public async Task<PayloadDTO> Execute(ActionDataPackageDTO curActionDataPackageDTO)
        {
            var processPayload = await GetProcessPayload(curActionDataPackageDTO.PayloadDTO.ProcessId);

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
                    JsonConvert.SerializeObject(payload),
                    CrateManifests.STANDARD_PAYLOAD_MANIFEST_NAME,
                    CrateManifests.STANDARD_PAYLOAD_MANIFEST_ID)
            };

            processPayload.UpdateCrateStorageDTO(cratesList);

            return processPayload;
        }

        public IList<FieldDTO> CreateActionPayload(ActionDTO curActionDO, string curEnvelopeId)
        {
            var curEnvelopeData = _docusignEnvelope.GetEnvelopeData(curEnvelopeId);
            var fields = GetFields(curActionDO);

            if (fields.Count == 0)
            {
                throw new InvalidOperationException("Field mappings are empty on ActionDO with id " + curActionDO.Id);
            }
            return _docusignEnvelope.ExtractPayload(fields, curEnvelopeId, curEnvelopeData);
        }

        private List<FieldDTO> GetFields(ActionDTO curActionDO)
        {
            var fieldsCrate = curActionDO.CrateStorage.CrateDTO
                .Where(x => x.ManifestType == CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME
                    && x.Label == "DocuSignTemplateUserDefinedFields")
                .FirstOrDefault();

            if (fieldsCrate == null) return null;

            var manifestSchema = JsonConvert.DeserializeObject<StandardDesignTimeFieldsMS>(fieldsCrate.Contents);

            if (manifestSchema == null
                || manifestSchema.Fields == null
                || manifestSchema.Fields.Count == 0)
            {
                return null;
            }

            return manifestSchema.Fields;
        }

        private string GetEnvelopeId(PayloadDTO curPayloadDTO)
        {
            var crate = curPayloadDTO.CrateStorageDTO().CrateDTO
                .SingleOrDefault(x => x.ManifestType == CrateManifests.STANDARD_PAYLOAD_MANIFEST_NAME);
            if (crate == null) return null; //TODO: log it
            
            var fields = JsonConvert.DeserializeObject<List<FieldDTO>>(crate.Contents);
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
                Label = "Docu Sign Envelope",
                Value = "This Action doesn't require any configuration.",
                cssClass = "well well-lg"
            };

            var crateControls = PackControlsCrate(textBlock);
            curActionDTO.CrateStorage.CrateDTO.Add(crateControls);

            // Extract upstream crates.
            List<CrateDTO> upstreamCrates;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curActivityDO = uow.ActivityRepository.GetByKey(curActionDTO.Id);
                upstreamCrates = GetCratesByDirection(
                    curActivityDO,
                    CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME,
                    GetCrateDirection.Upstream
                );
            }

            // Extract DocuSignTemplate Id.
            string docusignTemplateId = null;
            foreach (var crate in upstreamCrates)
            {
                var controlsMS = JsonConvert
                    .DeserializeObject<StandardConfigurationControlsMS>(crate.Contents);

                var control = controlsMS.Controls
                    .FirstOrDefault(x => x.Name == "Selected_DocuSign_Template");

                if (control != null)
                {
                    docusignTemplateId = control.Value;
                }
            }

            _crate.RemoveCrateByLabel(
                curActionDTO.CrateStorage.CrateDTO,
                "DocuSignTemplateUserDefinedFields"
                );

            // If DocuSignTemplate Id was found, then add design-time fields.
            if (!string.IsNullOrEmpty(docusignTemplateId))
            {
                var userDefinedFields = _docusignEnvelope
                    .GetEnvelopeDataByTemplate(docusignTemplateId);

                var fieldCollection = userDefinedFields
                    .Select(f => new FieldDTO()
                    {
                        Key = f.Name,
                        Value = f.Value
                    })
                    .ToArray();

                curActionDTO.CrateStorage.CrateDTO.Add(
                    _crate.CreateDesignTimeFieldsCrate(
                        "DocuSignTemplateUserDefinedFields",
                        fieldCollection
                        )
                    );
            }

            return curActionDTO;
        }
    }
}