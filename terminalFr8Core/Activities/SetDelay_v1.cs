using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
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

namespace terminalFr8Core.Activities
{
    public class SetDelay_v1 : BaseTerminalActivity
    {
        private const int MinDurationSeconds = 10;
        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetPayload(curActivityDO, containerId);
            using (var payloadStorage = CrateManager.UpdateStorage(() => curPayloadDTO.CrateStorage))
            {

                var operationsCrate = payloadStorage.CrateContentsOfType<OperationalStateCM>().FirstOrDefault();
                //check for operations crate
                if (operationsCrate == null)
                {
                    Error(payloadStorage, "This Action can't run without OperationalStateCM crate", ActivityErrorCode.PAYLOAD_DATA_MISSING);
                    return curPayloadDTO;
                }

                //find our action state in operations crate
                var delayState = operationsCrate.CallStack.GetLocalData<string>("Delay");

                //extract ActivityResponse type from result
                if (delayState == "suspended")
                {
                    //this is second time we are being called. this means alarm has triggered
                    Success(payloadStorage);
                    return curPayloadDTO;
                }

                //get user selected design time duration
                var delayDuration = GetUserDefinedDelayDuration(curActivityDO);
                var alarmDTO = CreateAlarm(curActivityDO, containerId, delayDuration);
                //post to hub to create an alarm
                await HubCommunicator.CreateAlarm(alarmDTO, CurrentFr8UserId);

                operationsCrate.CallStack.StoreLocalData("Delay", "suspended");
            }

            return SuspendHubExecution(curPayloadDTO);
            
        }

        private AlarmDTO CreateAlarm(ActivityDO activityDO, Guid containerId, TimeSpan duration)
        {
            if (duration.TotalSeconds == 0)
            {
                duration.Add(TimeSpan.FromSeconds(MinDurationSeconds));
            }
            return new AlarmDTO
            {
                ActivityDTO = Mapper.Map<ActivityDTO>(activityDO),
                ContainerId = containerId,
                TerminalName = "fr8Core",
                TerminalVersion = "v1",
                StartTime = DateTime.UtcNow.Add(duration)
            };
        }
        private TimeSpan GetUserDefinedDelayDuration(ActivityDO curActivityDO)
        {
            var controlsMS = CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().First();
            var manifestTypeDropdown = (Duration) controlsMS.Controls.Single(x => x.Type == ControlTypes.Duration && x.Name == "Delay_Duration");
            if (manifestTypeDropdown.Value == null)
            {
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "Delay activity can't create a delay without a selected duration on design time");
            }
            return manifestTypeDropdown.Value;
        }

        public override async Task<ActivityDO> Configure(ActivityDO curActionDataPackageDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActionDataPackageDO, ConfigurationEvaluator, authTokenDO);
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
        
            //build a controls crate to render the pane
            var configurationControlsCrate = CreateControlsCrate();

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Replace(AssembleCrateStorage(configurationControlsCrate));
            }

            return curActivityDO;
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

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            var controlsMS = CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

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