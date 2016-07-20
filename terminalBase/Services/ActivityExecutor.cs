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
        private IHubCommunicator _hubCommunicator;

        protected readonly ICrateManager CrateManager;
        
        public ActivityExecutor(IHubCommunicator hubCommunicator, ICrateManager crateManager)
        {
            _hubCommunicator = hubCommunicator;
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
                throw new ArgumentException(nameof(curDataDTO.ActivityDTO.ActivityTemplate) + " is null");
            }

           
            ActivityContext activityContext = DeserializeRequest(curDataDTO);
            
            //we should remove this mode
            var activityTemplate = curDataDTO.ActivityDTO.ActivityTemplate;
            IActivityFactory factory;


            if (activityTemplate.Name.EndsWith("_TEST"))
            {
                var originalName = activityTemplate.Name;

                _hubCommunicator = new TestMonitoringHubCommunicator(curDataDTO.ExplicitData);
                activityTemplate.Name = activityTemplate.Name.Substring(0, activityTemplate.Name.Length - "_TEST".Length);

                factory = ActivityStore.GetValue(activityTemplate);

                activityTemplate.Name = originalName;
            }
            else
            {
                factory = ActivityStore.GetValue(curDataDTO.ActivityDTO.ActivityTemplate);
            }
            

            if (factory == null)
            {
                throw new ArgumentException($"Activity template registration for [Name = '{curDataDTO.ActivityDTO.ActivityTemplate.Name}', Version = '{curDataDTO.ActivityDTO.ActivityTemplate.Version}']  not found");
            }

            var activity = factory.Create();

            _hubCommunicator.Configure(curTerminal, activityContext.UserId);

            activityContext.HubCommunicator = _hubCommunicator;

            ContainerExecutionContext executionContext;

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
                    executionContext = await CreateContainerExecutionContext(curDataDTO);
                    await activity.Run(activityContext, executionContext);
                    return SerializeResponse(executionContext);

                case "executechildactivities":
                    executionContext = await CreateContainerExecutionContext(curDataDTO);
                    await activity.RunChildActivities(activityContext, executionContext);
                    return SerializeResponse(executionContext);

                case "documentation":
                    var documentation = await activity.GetDocumentation(activityContext, curDataDTO.ActivityDTO.Documentation);
                    return SerializeResponse(documentation);

            }

            throw new ArgumentException("Unsupported request: " + curActionPath);
        }

        private async Task<ContainerExecutionContext> CreateContainerExecutionContext(Fr8DataDTO curDataDTO)
        {
            //this is just to keep integrations tests running
            //integration tests don't provide a containerid
            //we should modify integration tests
            //disabled for integration tests
            var containerId = curDataDTO.ContainerId ?? Guid.NewGuid();

            /*
            if (curDataDTO.ContainerId == null)
            {
                throw new ArgumentNullException(nameof(curDataDTO.ContainerId), "Container Id is missing");
            }
            */

            var payload = await _hubCommunicator.GetPayload(containerId);

            return new ContainerExecutionContext
            {
                PayloadStorage = CrateManager.GetUpdatableStorage(payload),
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

        private DocumentationResponseDTO SerializeResponse(DocumentationResponseDTO documentation)
        {
            return documentation;
        }
    }
}