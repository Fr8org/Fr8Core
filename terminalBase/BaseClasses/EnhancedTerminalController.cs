using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using TerminalBase.Infrastructure;

namespace TerminalBase.BaseClasses
{
    public class ActivityExecutionManager
    {
        protected readonly IHubCommunicator HubCommunicator;
        protected readonly ICrateManager CrateManager;
        protected ActivityExecutionManager(IHubCommunicator hubCommunicator, ICrateManager crateManager)
        {
            HubCommunicator = hubCommunicator;
            CrateManager = crateManager;
        }

        public async Task<object> HandleFr8Request(string curTerminal, string curActionPath, Fr8DataDTO curDataDTO)
        {
            IActivityFactory factory = ActivityStore.GetValue(curDataDTO.ActivityDTO.ActivityTemplate);
            if (factory == null)
            {
                throw new Exception("Activity template registration not found");
            }

            ActivityContext activityContext = DeserializeRequest(curDataDTO);
            IActivity activity;

            switch (curActionPath.ToLower())
            {
                case "configure":
                    activity = factory.Create(activityContext);
                    await activity.Configure();
                    return SerializeResponse(activityContext);

                case "activate":
                    activity = factory.Create(activityContext);
                    await activity.Activate();
                    return SerializeResponse(activityContext);

                case "run":
                    var executionContext = await CreateContainerExecutionContext(curDataDTO);
                    activity = factory.Create(activityContext, executionContext);
                    await activity.Run();
                    return SerializeResponse(executionContext);
            }

            throw new Exception("Unsupported request");
        }

        private async Task<ContainerExecutionContext> CreateContainerExecutionContext(Fr8DataDTO curDataDTO)
        {
            if (curDataDTO.ContainerId == null)
            {
                throw new ArgumentNullException(nameof(curDataDTO.ContainerId), "NULL Container ID");
            }

            var payload = await HubCommunicator.GetPayload(curDataDTO.ActivityDTO, curDataDTO.ContainerId.Value, curDataDTO.ActivityDTO.AuthToken.UserId);
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
