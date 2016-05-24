using System;
using System.Threading.Tasks;
using AutoMapper;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using StructureMap;
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
                var integrationTestMode = false;
                //we should remove this mode
                var activityTemplateName = curDataDTO.ActivityDTO.ActivityTemplate.Name;
                if (activityTemplateName.EndsWith("_TEST"))
                {
                    integrationTestMode = true;
                    curDataDTO.ActivityDTO.ActivityTemplate.Name = activityTemplateName.Substring(0, activityTemplateName.Length - "_TEST".Length);
                }
                //just created this for existing integration tests
                //we should find a smoother way for integration tests
                var hubCommunicator = integrationTestMode
                    ? new TestMonitoringHubCommunicator(curDataDTO.ExplicitData)
                    : ObjectFactory.GetInstance<IHubCommunicator>();
                

                IActivityFactory factory = ActivityStore.GetValue(curDataDTO.ActivityDTO.ActivityTemplate);
                if (factory == null)
                {
                    throw new ArgumentException("Activity template registration not found with name" + curDataDTO.ActivityDTO.ActivityTemplate.Name);
                }

                ActivityContext activityContext = DeserializeRequest(curDataDTO);
                IActivity activity = factory.Create();
                activity.SetHubCommunicator(hubCommunicator);
                switch (curActionPath.ToLower())
                {
                    case "configure":
                        await activity.Configure(activityContext);
                        return SerializeResponse(activityContext);

                    case "activate":
                        await activity.Activate(activityContext);
                        return SerializeResponse(activityContext);

                    case "deactivate":
                        await activity.Deactivate(activityContext);
                        return SerializeResponse(activityContext);

                    case "run":
                        await HubCommunicator.Configure(curTerminal);
                        var executionContext = await CreateContainerExecutionContext(curDataDTO);
                        await activity.Run(activityContext, executionContext);
                        return SerializeResponse(executionContext);

                    case "executechildactivities":
                        await HubCommunicator.Configure(curTerminal);
                        var executionContext2 = await CreateContainerExecutionContext(curDataDTO);
                        await activity.RunChildActivities(activityContext, executionContext2);
                        return SerializeResponse(executionContext2);
                }
            }
            catch (Exception e)
            {
                throw;
            }

            throw new ArgumentException("Unsupported request", nameof(curActionPath));
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
                PayloadStorage = CrateManager.GetStorage(payload),
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
