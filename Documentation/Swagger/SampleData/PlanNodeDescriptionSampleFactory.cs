using System;
using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects.PlanTemplates;

namespace HubWeb.Documentation.Swagger
{
    public class PlanNodeDescriptionSampleFactory : ISwaggerSampleFactory<PlanNodeDescriptionDTO>
    {
        private readonly ISwaggerSampleFactory<NodeTransitionDTO> _nodeTransitionSampleFactory;
        private readonly ISwaggerSampleFactory<ActivityDescriptionDTO> _activityDescriptionSampleFactory;

        public PlanNodeDescriptionSampleFactory(
            ISwaggerSampleFactory<NodeTransitionDTO> nodeTransitionSampleFactory, 
            ISwaggerSampleFactory<ActivityDescriptionDTO> activityDescriptionSampleFactory)
        {
            _nodeTransitionSampleFactory = nodeTransitionSampleFactory;
            _activityDescriptionSampleFactory = activityDescriptionSampleFactory;
        }

        public PlanNodeDescriptionDTO GetSampleData()
        {
            return new PlanNodeDescriptionDTO
            {
                Id = Guid.Parse("8AE37806-50BD-46A6-895B-8404952CE93F"),
                Name = "Plan Name",
                ActivityDescription = _activityDescriptionSampleFactory.GetSampleData(),
                IsStartingSubplan = true,
                ParentNodeId = Guid.Parse("5AAF1AA5-1BDB-4D4E-A26F-4E2B7D19EC37"),
                SubPlanName = "Starting Subplan",
                SubPlanOriginalId = "5A3618CA-3DBE-4296-A8C4-562C0AA6F590",
                Transitions = new List<NodeTransitionDTO> {  _nodeTransitionSampleFactory.GetSampleData() }
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}