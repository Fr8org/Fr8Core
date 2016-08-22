using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.TerminalBase.Helpers;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using StructureMap;

namespace Fr8.TerminalBase.Services
{
    public class ActivityExecutor : IActivityExecutor
    {
        private IHubCommunicator _hubCommunicator;

        protected readonly ICrateManager CrateManager;
        private readonly IActivityStore _activityStore;
        private readonly IContainer _container;

        public ActivityExecutor(IHubCommunicator hubCommunicator, ICrateManager crateManager, IActivityStore activityStore, IContainer container)
        {
            _hubCommunicator = hubCommunicator;
            CrateManager = crateManager;
            _activityStore = activityStore;
            _container = container;
        }

        public async Task<object> HandleFr8Request(
            string curActionPath,
            IEnumerable<KeyValuePair<string, string>> parameters,
            Fr8DataDTO curDataDTO)
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

                _hubCommunicator = new TestMonitoringHubCommunicator(curDataDTO.ExplicitData, CrateManager);
                activityTemplate.Name = activityTemplate.Name.Substring(0, activityTemplate.Name.Length - "_TEST".Length);

                factory = _activityStore.GetFactory(activityTemplate.Name, activityTemplate.Version);

                activityTemplate.Name = originalName;
            }
            else
            {
                factory = _activityStore.GetFactory(curDataDTO.ActivityDTO.ActivityTemplate.Name, curDataDTO.ActivityDTO.ActivityTemplate.Version);
            }


            if (factory == null)
            {
                throw new ArgumentException($"Activity template registration for [Name = '{curDataDTO.ActivityDTO.ActivityTemplate.Name}', Version = '{curDataDTO.ActivityDTO.ActivityTemplate.Version}']  not found");
            }

            _container.Configure(x =>
            {
                x.For<ActivityContext>().Use(activityContext);
                x.For<UpstreamQueryManager>().Use<UpstreamQueryManager>().Singleton();
            });

            var activity = factory.Create(_container);
            activityContext.HubCommunicator = _hubCommunicator;

            var scope = parameters != null && parameters.Any(x => x.Key == "scope")
                ? parameters.First(x => x.Key == "scope").Value
                : null;

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
                    {
                        var executionContext = await CreateContainerExecutionContext(curDataDTO);
                        if (scope == "childActivities")
                        {
                            await activity.RunChildActivities(activityContext, executionContext);
                        }
                        else
                        {
                            await activity.Run(activityContext, executionContext);
                        }
                        return SerializeResponse(executionContext);
                    }

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
                PayloadStorage = CrateStorageSerializer.Default.ConvertFromDto(payload?.CrateStorage),
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