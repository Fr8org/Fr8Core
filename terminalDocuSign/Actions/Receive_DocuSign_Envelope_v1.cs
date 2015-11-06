using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Enums;
using Hub.Interfaces;
using Hub.Managers;
using Newtonsoft.Json;
using StructureMap;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services;
using TerminalBase;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalDocuSign.Actions
{
    public class Receive_DocuSign_Envelope_v1 : BasePluginAction
    {
        private readonly DocuSignManager _docuSignManager;
        private readonly IRouteNode _routeNode;

        public Receive_DocuSign_Envelope_v1()
        {
            _routeNode = ObjectFactory.GetInstance<IRouteNode>();
            _docuSignManager = new DocuSignManager();
        }

        public async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            return await ProcessConfigurationRequest(curActionDO, dto => ConfigurationRequestType.Initial, authTokenDO);
        }

        public void Activate(ActionDO curActionDO)
        {
            return; // Will be changed when implementation is plumbed in.
        }

        public void Deactivate(ActionDO curActionDO)
        {
            return; // Will be changed when implementation is plumbed in.
        }

        public async Task<PayloadDTO> Run(ActionDO actionDO, int containerId, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            var processPayload = await GetProcessPayload(containerId);

            //Get envlopeId
            string envelopeId = GetEnvelopeId(processPayload);
            if (envelopeId == null)
            {
                throw new PluginCodedException(PluginErrorCode.PAYLOAD_DATA_MISSING, "EnvelopeId");
            }

            using (var updater = Crate.UpdateStorage(() => processPayload.CrateStorage))
            {
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("DocuSign Envelope Data", CreateActionPayload(actionDO), envelopeId)));
            }

            return processPayload;
        }

        public IList<FieldDTO> CreateActionPayload(ActionDO curActionDO, AuthorizationTokenDO authTokenDO, string curEnvelopeId)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthDTO>(authTokenDO.Token);

            var docusignEnvelope = new DocuSignEnvelope(
                docuSignAuthDTO.Email,
                docuSignAuthDTO.ApiPassword);

            var curEnvelopeData = docusignEnvelope.GetEnvelopeData(curEnvelopeId);
            var fields = GetFields(curActionDO);

            if (fields == null || fields.Count == 0)
            {
                throw new InvalidOperationException("Field mappings are empty on ActionDO with id " + curActionDO.Id);
            }

            return new StandardPayloadDataCM(docusignEnvelope.ExtractPayload(fields, curEnvelopeId, curEnvelopeData));
        }

        private List<FieldDTO> GetFields(ActionDO curActionDO)
        {
            var fieldsCrate = Crate.FromDto(curActionDO.CrateStorage).CratesOfType<StandardDesignTimeFieldsCM>().FirstOrDefault(x => x.Label == "DocuSignTemplateUserDefinedFields");

            if (fieldsCrate == null) return null;

            var manifestSchema = fieldsCrate.Content;

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
            var standardPayload = Crate.FromDto(curPayloadDTO.CrateStorage).CrateContentsOfType<StandardPayloadDataCM>().FirstOrDefault();

            if (standardPayload == null)
            {
                return null;
            }

            var envelopeId = standardPayload.GetValues("EnvelopeId").FirstOrDefault();

            return envelopeId;
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO = null)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthDTO>(authTokenDO.Token);

            //get envelopeIdFromUpstreamActions
            var upstream = await _routeNode.GetCratesByDirection<StandardDesignTimeFieldsCM>(curActionDO.Id, GetCrateDirection.Upstream);

            var envelopeId = upstream.SelectMany(x => x.Content.Fields).FirstOrDefault(x => x.Key == "EnvelopeId");

            //In order to Receive a DocuSign Envelope as fr8, an upstream action needs to provide a DocuSign EnvelopeID.
            TextBlockControlDefinitionDTO textBlock;
            if (envelopeId != null)
            {
                textBlock = new TextBlockControlDefinitionDTO
                {
                    Label = "Docu Sign Envelope",
                    Value = "This Action doesn't require any configuration.",
                    CssClass = "well well-lg"
                };
            }
            else
            {
                textBlock = new TextBlockControlDefinitionDTO
                {
                    Label = "Docu Sign Envelope",
                    Value = "In order to Receive a DocuSign Envelope as fr8, an upstream action needs to provide a DocuSign EnvelopeID.",
                    CssClass = "alert alert-warning"
                };
            }

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(PackControlsCrate(textBlock));
            }

            // var templateId = upstream.SelectMany(x => x.Content.Fields).FirstOrDefault(x => x.Key == "TemplateId");
            var templateId = envelopeId.Value;

            // If DocuSignTemplate Id was found, then add design-time fields.
            _docuSignManager.ExtractFieldsAndAddToCrate(docuSignTemplateId, docuSignAuthDTO, curActionDO);

            return curActionDO;
        }
    }
}