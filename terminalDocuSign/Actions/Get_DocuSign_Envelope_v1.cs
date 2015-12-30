
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
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
using Utilities;

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
            var payloadCrates = await GetPayload(actionDO, containerId);

            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(payloadCrates);
            }

            //Get envlopeId
            var control = FindControl(Crate.GetStorage(actionDO), "EnvelopeIdSelector");
            string envelopeId = GetEnvelopeID(control, authTokenDO);
            if (envelopeId == null)
            {
                return Error(payloadCrates, "EnvelopeId", ActionErrorCode.PAYLOAD_DATA_MISSING);
            }

            using (var updater = Crate.UpdateStorage(() => payloadCrates.CrateStorage))
            {
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("DocuSign Envelope Data", _docuSignManager.CreateActionPayload(actionDO, authTokenDO, envelopeId)));
            }

            return Success(payloadCrates);
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
            var control = CreateSpecificOrUpstreamValueChooser(
               "TemplateId or TemplateName",
               "EnvelopeIdSelector",
               "Upstream Design-Time Fields"
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
                var curUpstreamFields = (await GetDesignTimeFields(curActionDO, CrateDirection.Upstream)).Fields.ToArray();
                var upstreamFieldsCrate = Crate.CreateDesignTimeFieldsCrate("Upstream Design-Time Fields", curUpstreamFields);
                updater.CrateStorage.ReplaceByLabel(upstreamFieldsCrate);

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
            string envelopeId = "";
            var textSource = (TextSource)control;
            if (textSource.ValueSource == null)
                return null;

            var realTSValue = textSource.ValueSource == "specific" ? textSource.TextValue : textSource.Value;

            //Gets EnvelopeId either by EnvelopeId or TemplateName
            if (!string.IsNullOrEmpty(realTSValue))
            {
                if (realTSValue.IsGuid())
                {
                    envelopeId = realTSValue;
                }
                else
                {
                    var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuth>(authTokenDo.Token);
                    var availableTemplates = (_docuSignManager.PackCrate_DocuSignTemplateNames(docuSignAuthDTO).Get()
                        as StandardDesignTimeFieldsCM).Fields;
                    var selectedTemplate = availableTemplates.FirstOrDefault(a => a.Key.ToLowerInvariant() == realTSValue.ToLowerInvariant());
                    envelopeId = (selectedTemplate != null) ? selectedTemplate.Value : "";
                }
            }
            return envelopeId;
        }
    }
}