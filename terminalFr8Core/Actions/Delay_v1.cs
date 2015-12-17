using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using TerminalBase;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities;

namespace terminalFr8Core.Actions
{
    public class Delay_v1 : BaseTerminalAction
    {
        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetProcessPayload(curActionDO, containerId);
            var payloadStorage = Crate.GetStorage(curPayloadDTO);

            var loopId = curActionDO.Id.ToString();
            var operationsCrate = payloadStorage.CrateContentsOfType<OperationalStateCM>().FirstOrDefault();
            if (operationsCrate == null)
            {
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "This Action can't run without OperationalStatusCM crate");
            }
            //set default loop index for initial state
            var currentLoopIndex = 0;
            var myLoop = operationsCrate.Loops.FirstOrDefault(l => l.Id == loopId);
            if (myLoop == null)
            {
                CreateLoop(curActionDO.Id.ToString(), curPayloadDTO);
            }
            else
            {
                currentLoopIndex = 0;
            }

            //get user selected design time values
            var manifestType = GetSelectedCrateManifestTypeToProcess(curActionDO);
            var label = GetSelectedLabelToProcess(curActionDO);

            //find crate by user selected values
            var crateToProcess = payloadStorage.FirstOrDefault(c => /*c.ManifestType.Type == manifestType && */c.Label == label);

            if (crateToProcess == null)
            {
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "Unable to find any crate with Manifest Type: \"" + manifestType + "\" and Label: \"" + label + "\"");
            }

            Object[] dataList = null;

            if (dataList == null)
            {
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "Unable to find a list in specified crate with Manifest Type: \"" + manifestType + "\" and Label: \"" + label + "\"");
            }

            //check if we need to end this loop
            if (currentLoopIndex > dataList.Length - 1)
            {
                BreakLoop(curActionDO.Id.ToString(), curPayloadDTO);
            }

            return curPayloadDTO;
        }

        private void BreakLoop(string loopId, PayloadDTO payload)
        {
            using (var updater = Crate.UpdateStorage(payload))
            {
                var operationsData = updater.CrateStorage.CrateContentsOfType<OperationalStateCM>().Single();
                operationsData.Loops.Single(l => l.Id == loopId).BreakSignalReceived = true;
            }
        }

        private void CreateLoop(string loopId, PayloadDTO payload)
        {
            using (var updater = Crate.UpdateStorage(payload))
            {
                var operationalState = updater.CrateStorage.CrateContentsOfType<OperationalStateCM>().Single();
                var loopLevel = operationalState.Loops.Count(l => l.BreakSignalReceived == false);
                operationalState.Loops.Add(new OperationalStateCM.LoopStatus
                {
                    BreakSignalReceived = false,
                    Id = loopId,
                    Index = 0,
                    Level = loopLevel
                });
            }
        }


        private string GetSelectedCrateManifestTypeToProcess(ActionDO curActionDO)
        {
            var controlsMS = Crate.GetStorage(curActionDO).CrateContentsOfType<StandardConfigurationControlsCM>().First();
            var manifestTypeDropdown = controlsMS.Controls.Single(x => x.Type == ControlTypes.DropDownList && x.Name == "Available_Manifests");
            if (manifestTypeDropdown.Value == null)
            {
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "Loop action can't process data without a selected Manifest Type to process");
            }
            return manifestTypeDropdown.Value;
        }

        private string GetSelectedLabelToProcess(ActionDO curActionDO)
        {
            var controlsMS = Crate.GetStorage(curActionDO).CrateContentsOfType<StandardConfigurationControlsCM>().First();
            var labelDropdown = controlsMS.Controls.Single(x => x.Type == ControlTypes.DropDownList && x.Name == "Available_Labels");
            if (labelDropdown.Value == null)
            {
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "Loop action can't process data without a selected Label to process");
            }
            return labelDropdown.Value;
        }

        public override async Task<ActionDO> Configure(ActionDO curActionDataPackageDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActionDataPackageDO, ConfigurationEvaluator, authTokenDO);
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            //build a controls crate to render the pane
            var configurationControlsCrate = CreateControlsCrate();

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage = AssembleCrateStorage(configurationControlsCrate);
            }

            return curActionDO;
        }

        private Crate CreateControlsCrate()
        {
            var duration = new Duration
            {
                Label = "Please enter delay duration",
                Name = "Delay_Duration"
            };


            return PackControlsCrate(duration);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            var controlsMS = Crate.GetStorage(curActionDO).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (controlsMS == null)
            {
                return ConfigurationRequestType.Initial;
            }

            var durationControl = controlsMS.Controls.FirstOrDefault(x => x.Type == ControlTypes.Duration && x.Name == "Delay_Duration");

            if (durationControl == null)
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
    }
}