using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
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

            var actionId = curActionDO.Id.ToString();
            var operationsCrate = payloadStorage.CrateContentsOfType<OperationalStateCM>().FirstOrDefault();
            //check for operations crate
            if (operationsCrate == null)
            {
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "This Action can't run without OperationalStateCM crate");
            }
            
            //find our action state in operations crate
            var myState = operationsCrate.States.FirstOrDefault(l => l.Id == actionId);
            if (myState == null)
            {
                //this is first time we are being called
                CreatePendingState(actionId, curPayloadDTO);

                //get user selected design time duration
                var delayDuration = GetUserDefinedDelayDuration(curActionDO);
                var alarmDTO = CreateAlarm(curActionDO, containerId, delayDuration);
                //post to hub to create an alarm
                await HubCommunicator.CreateAlarm(alarmDTO);
            }
            else
            {
                //this is second time we are being called. this means alarm has triggered
                MarkActionAsCompleted(actionId, curPayloadDTO);
            }

            return curPayloadDTO;
        }

        private AlarmDTO CreateAlarm(ActionDO actionDO, Guid containerId, TimeSpan duration)
        {
            return new AlarmDTO
            {
                ActionDTO = Mapper.Map<ActionDTO>(actionDO),
                ContainerId = containerId,
                TerminalName = "fr8Core",
                TerminalVersion = "v1",
                StartTime = DateTime.UtcNow.Add(duration)
            };
        }

        private void CreatePendingState(string actionId, PayloadDTO payload)
        {
            using (var updater = Crate.UpdateStorage(payload))
            {
                var operationalState = updater.CrateStorage.CrateContentsOfType<OperationalStateCM>().Single();
                operationalState.States.Add(new OperationalStateCM.ActionStateMatch
                {
                    Id = actionId,
                    State = ActionState.Pending
                });
            }
        }

        private void MarkActionAsCompleted(string actionId, PayloadDTO payload)
        {
            using (var updater = Crate.UpdateStorage(payload))
            {
                var operationalState = updater.CrateStorage.CrateContentsOfType<OperationalStateCM>().Single();
                var actionState = operationalState.States.First(s => s.Id == actionId);
                actionState.State = ActionState.Completed;
            }
        }


        private TimeSpan GetUserDefinedDelayDuration(ActionDO curActionDO)
        {
            var controlsMS = Crate.GetStorage(curActionDO).CrateContentsOfType<StandardConfigurationControlsCM>().First();
            var manifestTypeDropdown = (Duration) controlsMS.Controls.Single(x => x.Type == ControlTypes.Duration && x.Name == "Delay_Duration");
            if (manifestTypeDropdown.Value == null)
            {
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "Delay action can't create a delay without a selected duration on design time");
            }
            return manifestTypeDropdown.Value;
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