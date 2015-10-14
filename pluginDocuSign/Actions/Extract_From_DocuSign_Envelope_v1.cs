using PluginBase.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using Core.Interfaces;
using PluginBase;
using Data.Interfaces.ManifestSchemas;
using System.Threading.Tasks;
using Core.Enums;
using pluginDocuSign.DataTransferObjects;
using pluginDocuSign.Services;
using PluginUtilities.BaseClasses;

namespace pluginDocuSign.Actions
{
    public class Extract_From_DocuSign_Envelope_v1 : BasePluginAction
    {
        // TODO: remove this as of DO-1064
        // IDocuSignEnvelope _docusignEnvelope = ObjectFactory.GetInstance<IDocuSignEnvelope>();

        public Extract_From_DocuSign_Envelope_v1()
        {
            // TODO: remove this as of DO-1064
            // _docusignEnvelope = ObjectFactory.GetInstance<IDocuSignEnvelope>();
        }

        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            if (IsEmptyAuthToken(curActionDTO))
            {
                AppendDockyardAuthenticationCrate(curActionDTO, AuthenticationMode.InternalMode);
                return curActionDTO;
            }

            RemoveAuthenticationCrate(curActionDTO);

            return await ProcessConfigurationRequest(curActionDTO, actionDo => ConfigurationRequestType.Initial);
        }

        public void Activate(ActionDTO curActionDTO)
        {
            return; // Will be changed when implementation is plumbed in.
        }

        public void Deactivate(ActionDTO curActionDTO)
        {
            return; // Will be changed when implementation is plumbed in.
        }

        public async Task<PayloadDTO> Execute(ActionDTO actionDto)
        {
            if (IsEmptyAuthToken(actionDto))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            var processPayload = await GetProcessPayload(actionDto.ProcessId);

            //Get envlopeId
            string envelopeId = GetEnvelopeId(processPayload);
            if (envelopeId == null)
            {
                throw new PluginCodedException(PluginErrorCode.PAYLOAD_DATA_MISSING, "EnvelopeId");
            }

            var payload = CreateActionPayload(actionDto, envelopeId);
            var cratesList = new List<CrateDTO>()
            {
                Crate.Create("DocuSign Envelope Data",
                    JsonConvert.SerializeObject(payload),
                    CrateManifests.STANDARD_PAYLOAD_MANIFEST_NAME,
                    CrateManifests.STANDARD_PAYLOAD_MANIFEST_ID)
            };

            processPayload.UpdateCrateStorageDTO(cratesList);

            return processPayload;
        }

        public IList<FieldDTO> CreateActionPayload(ActionDTO curActionDTO, string curEnvelopeId)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthDTO>(curActionDTO.AuthToken.Token);

            var docusignEnvelope = new DocuSignEnvelope(
                docuSignAuthDTO.Email,
                docuSignAuthDTO.ApiPassword);

            var curEnvelopeData = docusignEnvelope.GetEnvelopeData(curEnvelopeId);
            var fields = GetFields(curActionDTO);

            if (fields == null || fields.Count == 0)
            {
                throw new InvalidOperationException("Field mappings are empty on ActionDO with id " + curActionDTO.Id);
            }

            return docusignEnvelope.ExtractPayload(fields, curEnvelopeId, curEnvelopeData);
        }

        private List<FieldDTO> GetFields(ActionDTO curActionDO)
        {
            var fieldsCrate = curActionDO.CrateStorage.CrateDTO
                .Where(x => x.ManifestType == CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME
                    && x.Label == "DocuSignTemplateUserDefinedFields")
                .FirstOrDefault();

            if (fieldsCrate == null) return null;

            var manifestSchema = JsonConvert.DeserializeObject<StandardDesignTimeFieldsCM>(fieldsCrate.Contents);

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

        protected override async Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthDTO>(
                curActionDTO.AuthToken.Token);

            // "[{ type: 'textField', name: 'connection_string', required: true, value: '', fieldLabel: 'SQL Connection String' }]"
            var textBlock = new TextBlockControlDefinitionDTO()
            {
                Label = "Docu Sign Envelope",
                Value = "This Action doesn't require any configuration.",
                CssClass = "well well-lg"
            };

            var crateControls = PackControlsCrate(textBlock);
            curActionDTO.CrateStorage.CrateDTO.Add(crateControls);
            List<CrateDTO> upstreamCrates = new List<CrateDTO>();

            // Extract upstream crates.
            upstreamCrates = await GetCratesByDirection(
                curActionDTO.Id,
                CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME,
                GetCrateDirection.Upstream
            );


            // Extract DocuSignTemplate Id.
            string docusignTemplateId = null;
            foreach (var crate in upstreamCrates)
            {
                var controlsMS = JsonConvert
                    .DeserializeObject<StandardConfigurationControlsCM>(crate.Contents);

                var control = controlsMS.Controls
                    .FirstOrDefault(x => x.Name == "Selected_DocuSign_Template");

                if (control != null)
                {
                    docusignTemplateId = control.Value;
                }
            }


            Crate.RemoveCrateByLabel(
                curActionDTO.CrateStorage.CrateDTO,
                "DocuSignTemplateUserDefinedFields"
                );

            // If DocuSignTemplate Id was found, then add design-time fields.
            if (!string.IsNullOrEmpty(docusignTemplateId))
            {
                var docusignEnvelope = new DocuSignEnvelope(
                    docuSignAuthDTO.Email, docuSignAuthDTO.ApiPassword);

                var userDefinedFields = docusignEnvelope
                    .GetEnvelopeDataByTemplate(docusignTemplateId);

                var fieldCollection = userDefinedFields
                    .Select(f => new FieldDTO()
                    {
                        Key = f.Name,
                        Value = f.Value
                    })
                    .ToArray();

                curActionDTO.CrateStorage.CrateDTO.Add(
                    Crate.CreateDesignTimeFieldsCrate(
                        "DocuSignTemplateUserDefinedFields",
                        fieldCollection
                        )
                    );
            }

            return curActionDTO;
        }
    }
}