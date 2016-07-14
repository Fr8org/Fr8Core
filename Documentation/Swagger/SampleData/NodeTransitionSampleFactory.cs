using System;
using Fr8.Infrastructure.Data.DataTransferObjects.PlanTemplates;

namespace HubWeb.Documentation.Swagger
{
    public class NodeTransitionSampleFactory : ISwaggerSampleFactory<NodeTransitionDTO>
    {
        public NodeTransitionDTO GetSampleData()
        {
            return new NodeTransitionDTO
            {
                Id = Guid.Parse("6546C300-8B39-4A14-8D75-734DE61D2739"),
                PlanId = Guid.Parse("7687EE50-4517-4056-8EFD-6650EFA82A46"),
                ActivityDescriptionId = Guid.Empty,
                Transition = "Launch Additional Plan"
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}