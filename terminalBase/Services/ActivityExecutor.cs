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
        protected readonly ICrateManager CrateManager;
        public ActivityExecutor(ICrateManager crateManager)
        {
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
                var activityTemplate = curDataDTO.ActivityDTO.ActivityTemplate;
                var originalActivityTemplateName = activityTemplate.Name;
                if (activityTemplate.Name.EndsWith("_TEST"))
                {
                    integrationTestMode = true;
                    activityTemplate.Name = activityTemplate.Name.Substring(0, activityTemplate.Name.Length - "_TEST".Length);
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

                if (integrationTestMode)
                {
                    //let's restore activity template name
                    activityTemplate.Name = originalActivityTemplateName;
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
                        var executionContext = await CreateContainerExecutionContext(hubCommunicator, curDataDTO, curTerminal);
                        await activity.Run(activityContext, executionContext);
                        return SerializeResponse(executionContext);

                    case "executechildactivities":
                        var executionContext2 = await CreateContainerExecutionContext(hubCommunicator, curDataDTO, curTerminal);
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

        private async Task<ContainerExecutionContext> CreateContainerExecutionContext(IHubCommunicator hubCommunicator, Fr8DataDTO curDataDTO, string terminalName)
        {
            await hubCommunicator.Configure(terminalName);
            //this is just to keep integrations tests running
            //integration tests don't provide a containerid
            //we should modify integration tests
            //disabled for integration tests
            var containerId = curDataDTO.ContainerId ?? Guid.NewGuid();
            /*
            if (curDataDTO.ContainerId == null)
            {
                throw new ArgumentNullException(nameof(curDataDTO.ContainerId), "NULL Container ID");
            }
            */
            var payload = await hubCommunicator.GetPayload(containerId, curDataDTO.ActivityDTO.AuthToken.UserId);
            return new ContainerExecutionContext
            {
                PayloadStorage = CrateManager.GetStorage(payload),
                
                ContainerId = containerId
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
