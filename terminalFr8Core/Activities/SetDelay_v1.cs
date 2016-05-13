using System;
using System.Threading.Tasks;
using AutoMapper;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using TerminalBase;
using TerminalBase.BaseClasses;

namespace terminalFr8Core.Activities
{
    public class SetDelay_v1 : BaseTerminalActivity
    {
        private const int MinDurationSeconds = 10;
        private AlarmDTO CreateAlarm(TimeSpan duration)
        {
            if (duration.TotalSeconds == 0)
            {
                duration.Add(TimeSpan.FromSeconds(MinDurationSeconds));
            }
            return new AlarmDTO
            {
                ActivityDTO = Mapper.Map<ActivityDTO>(ActivityContext.ActivityPayload),
                ContainerId = ExecutionContext.ContainerId,
                TerminalName = TerminalData.TerminalDTO.Name,
                TerminalVersion = TerminalData.TerminalDTO.Version,
                StartTime = DateTime.UtcNow.Add(duration)
            };
        }
        private TimeSpan GetUserDefinedDelayDuration()
        {
            var manifestTypeDropdown = GetControl<Duration>("Delay_Duration");
            if (manifestTypeDropdown.Value == null)
            {
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "Delay activity can't create a delay without a selected duration on design time");
            }
            return manifestTypeDropdown.Value;
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

        public SetDelay_v1() : base(false)
        {
        }

        protected override ActivityTemplateDTO MyTemplate { get; }
        public override async Task Run()
        {
            //find our action state in operations crate
            var delayState = OperationalState.CallStack.GetLocalData<string>("Delay");
            //extract ActivityResponse type from result
            if (delayState == "suspended")
            {
                //this is second time we are being called. this means alarm has triggered
                Success();
                return;
            }

            //get user selected design time duration
            var delayDuration = GetUserDefinedDelayDuration();
            var alarmDTO = CreateAlarm(delayDuration);
            //post to hub to create an alarm
            await HubCommunicator.CreateAlarm(alarmDTO, CurrentUserId);

            OperationalState.CallStack.StoreLocalData("Delay", "suspended");

            SuspendHubExecution();
        }

        public override Task Initialize()
        {
            var configurationControlsCrate = CreateControlsCrate();
            Storage.Add(configurationControlsCrate);
            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }
    }
}