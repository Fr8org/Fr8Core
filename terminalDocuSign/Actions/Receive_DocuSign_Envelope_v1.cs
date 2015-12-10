
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
﻿using Data.Control;
﻿using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;

using Hub.Interfaces;
using Hub.Managers;
using Newtonsoft.Json;
using StructureMap;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services;
using TerminalBase;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Data.Entities;
using Data.Crates;
﻿using Data.States;

namespace terminalDocuSign.Actions
{
    public class Receive_DocuSign_Envelope_v1 : BaseTerminalAction
    {
        private readonly DocuSignManager _docuSignManager;

        public Receive_DocuSign_Envelope_v1()
        {
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

        public async Task<PayloadDTO> Run(ActionDO actionDO,
            Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            var processPayload = await GetProcessPayload(actionDO, containerId);

            //Get envlopeId
            string envelopeId = GetEnvelopeId(processPayload);
            if (envelopeId == null)
            {
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "EnvelopeId");
            }

            using (var updater = Crate.UpdateStorage(() => processPayload.CrateStorage))
            {
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("DocuSign Envelope Data", CreateActionPayload(actionDO, authTokenDO, envelopeId)));
            }

            return processPayload;
        }

        public StandardPayloadDataCM CreateActionPayload(ActionDO curActionDO, AuthorizationTokenDO authTokenDO, string curEnvelopeId)
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
            var fieldsCrate = Crate.GetStorage(curActionDO).CratesOfType<StandardDesignTimeFieldsCM>().FirstOrDefault(x => x.Label == "DocuSignTemplateUserDefinedFields");

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

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthDTO>(authTokenDO.Token);

            //get envelopeIdFromUpstreamActions
            var upstream = await GetCratesByDirection<StandardDesignTimeFieldsCM>(curActionDO, CrateDirection.Upstream);

            var templateId = upstream.SelectMany(x => x.Content.Fields).FirstOrDefault(x => x.Key == "TemplateId");

            //In order to Receive a DocuSign Envelope as fr8, an upstream action needs to provide a DocuSign EnvelopeID.
            TextBlock textBlock;
            if (templateId != null)
            {
                textBlock = new TextBlock
                {
                    Label = "Docu Sign Envelope",
                    Value = "This Action doesn't require any configuration.",
                    CssClass = "well well-lg"
                };
            }
            else
            {
                textBlock = new TextBlock
                {
                    Label = "Docu Sign Envelope",
                    Value = "In order to Receive a DocuSign Envelope as fr8, an upstream action needs to provide a DocuSign TemplateId.",
                    CssClass = "alert alert-warning"
                };
            }
            
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(PackControlsCrate(textBlock));
            }

            // var templateId = upstream.SelectMany(x => x.Content.Fields).FirstOrDefault(x => x.Key == "TemplateId");
            //var templateId = templateId.Value;

            // If DocuSignTemplate Id was found, then add design-time fields.
            if (templateId != null && !string.IsNullOrEmpty(templateId.Value))
            {
                _docuSignManager.ExtractFieldsAndAddToCrate(templateId.Value, docuSignAuthDTO, curActionDO);
            }
            return curActionDO;
        }
    }
}