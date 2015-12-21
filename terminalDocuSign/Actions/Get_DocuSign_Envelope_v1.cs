
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Interfaces.DataTransferObjects;
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
using Data.States;

namespace terminalDocuSign.Actions
{
    public class Get_DocuSign_Envelope_v1 : BaseTerminalAction
    {
        private readonly DocuSignManager _docuSignManager;

        public Get_DocuSign_Envelope_v1()
        {
            _docuSignManager = new DocuSignManager();
        }

        public async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            return await ProcessConfigurationRequest(curActionDO, dto => ConfigurationEvaluator(dto), authTokenDO);
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
            var control = FindControl(Crate.GetStorage(actionDO), "EnvelopeIdSelector");
            string envelopeId = GetEnvelopeID(control, authTokenDO);
            if (envelopeId == null)
            {
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "EnvelopeId");
            }

            using (var updater = Crate.UpdateStorage(() => processPayload.CrateStorage))
            {
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("DocuSign Envelope Data", _docuSignManager.CreateActionPayload(actionDO, authTokenDO, envelopeId)));
            }

            return processPayload;
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuth>(authTokenDO.Token);

            //get envelopeIdFromUpstreamActions
            var upstream = await GetCratesByDirection<StandardDesignTimeFieldsCM>(curActionDO, CrateDirection.Upstream);

            var control = CreateSpecificOrUpstreamValueChooser(
               "TemplateId or TemplateName",
               "EnvelopeIdSelector",
               "Upstream Terminal-Provided Design-Time Fields"
            );

            control.Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") };

            var info_label = new TextBlock() { Name = "Info", CssClass = "well well-lg" };

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(PackControlsCrate(control, info_label));

            }

            return await Task.FromResult<ActionDO>(curActionDO); ;
        }

        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.RemoveByLabel("Upstream Terminal-Provided Design-Time Fields");
                var curUpstreamFields = (await GetDesignTimeFields(curActionDO, CrateDirection.Upstream)).Fields.ToArray();
                var upstreamFieldsCrate = Crate.CreateDesignTimeFieldsCrate("Upstream Terminal-Provided Design-Time Fields", curUpstreamFields);
                updater.CrateStorage.Add(upstreamFieldsCrate);

                var control = FindControl(Crate.GetStorage(curActionDO), "EnvelopeIdSelector");
                string envelopeId = GetEnvelopeID(control as TextSource, authTokenDO);
                int fieldsCount = _docuSignManager.UpdateUserDefinedFields(curActionDO, authTokenDO, updater, envelopeId);

                var info_label = FindControl(updater.CrateStorage, "Info");
                if (String.IsNullOrEmpty(envelopeId))
                    info_label.Value = "Couldn't find Envelope by provided value";
                else
                    info_label.Value = String.Format("Found {0} field(s)", fieldsCount);

            }
            return await Task.FromResult(curActionDO);
        }

        private string GetEnvelopeID(ControlDefinitionDTO control, AuthorizationTokenDO authTokenDo)
        {
            TextSource textSource = (TextSource)control;
            if (textSource.ValueSource == null || string.IsNullOrEmpty(textSource.Value))
                return null;

            string result = "";
            Guid envelopeId;
            if (Guid.TryParse(control.Value, out envelopeId))
                result = envelopeId.ToString();
            else
            {
                var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuth>(authTokenDo.Token);
                var selectedTemplate = (_docuSignManager.PackCrate_DocuSignTemplateNames(docuSignAuthDTO).Get()
                    as StandardDesignTimeFieldsCM).Fields.Where(a => a.Key.ToLowerInvariant() == control.Value.ToLowerInvariant()).FirstOrDefault();
                result = (selectedTemplate != null) ? selectedTemplate.Value : "";
            }
            return result;
        }
    }
}