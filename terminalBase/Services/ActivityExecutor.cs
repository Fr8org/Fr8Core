using System;
using System.Threading.Tasks;
using AutoMapper;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using TerminalBase.Infrastructure;
using TerminalBase.Interfaces;
using TerminalBase.Models;

namespace TerminalBase.Services
{
    public class ActivityExecutor
    {
        protected readonly IHubCommunicator HubCommunicator;
        protected readonly ICrateManager CrateManager;
        public ActivityExecutor(IHubCommunicator hubCommunicator, ICrateManager crateManager)
        {
            HubCommunicator = hubCommunicator;
            CrateManager = crateManager;
        }

        public async Task<object> HandleFr8Request(string curTerminal, string curActionPath, Fr8DataDTO curDataDTO)
        {
            if (curDataDTO == null)
            {
                throw new ArgumentNullException(nameof(curDataDTO));
            }
            if (curDataDTO.ActivityDTO == null)
            {
                throw new ArgumentException(nameof(curDataDTO.ActivityDTO) + " is null");
            }
            if (curDataDTO.ActivityDTO.ActivityTemplate == null)
            {
                throw new ArgumentException(nameof(curDataDTO.ActivityDTO.ActivityTemplate) +" is null");
            }

            try
            {
                IActivityFactory factory = ActivityStore.GetValue(curDataDTO.ActivityDTO.ActivityTemplate);
                if (factory == null)
                {
                    throw new ArgumentException("Activity template registration not found with name" + curDataDTO.ActivityDTO.ActivityTemplate.Name);
                }

                ActivityContext activityContext = DeserializeRequest(curDataDTO);
                IActivity activity;

                switch (curActionPath.ToLower())
                {
                    case "configure":
                        activity = factory.Create();
                        await activity.Configure(activityContext);
                        return SerializeResponse(activityContext);

                    case "activate":
                        activity = factory.Create();
                        await activity.Activate(activityContext);
                        return SerializeResponse(activityContext);

                    case "deactivate":
                        activity = factory.Create();
                        await activity.Deactivate(activityContext);
                        return SerializeResponse(activityContext);

                    case "run":
                        await HubCommunicator.Configure(curTerminal);
                        var executionContext = await CreateContainerExecutionContext(curDataDTO);
                        activity = factory.Create();
                        await activity.Run(activityContext, executionContext);
                        return SerializeResponse(executionContext);
                }
            }
            catch (Exception e)
            {
                throw;
            }

            throw new Exception("Unsupported request");
        }

        private async Task<ContainerExecutionContext> CreateContainerExecutionContext(Fr8DataDTO curDataDTO)
        {
            if (curDataDTO.ContainerId == null)
            {
                throw new ArgumentNullException(nameof(curDataDTO.ContainerId), "NULL Container ID");
            }

            var payload = await HubCommunicator.GetPayload(curDataDTO.ContainerId.Value, curDataDTO.ActivityDTO.AuthToken.UserId);
            return new ContainerExecutionContext
            {
                PayloadStorage = CrateManager.GetUpdatableStorage(payload),
                ContainerId = curDataDTO.ContainerId.Value
            };
        }

        private ActivityContext DeserializeRequest(Fr8DataDTO activityDto)
        {
            return Mapper.Map<ActivityContext>(activityDto);
        }

        private ActivityDTO SerializeResponse(ActivityContext activityContext)
        {
            return Mapper.Map<ActivityDTO>(activityContext.ActivityPayload);
        }

        private PayloadDTO SerializeResponse(ContainerExecutionContext activityContext)
        {
            return Mapper.Map<PayloadDTO>(activityContext);
        }
    }
}
