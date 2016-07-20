using System;
using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects.PlanTemplates;

namespace HubWeb.Documentation.Swagger
{
    public class PlanTemplateSampleFactory : ISwaggerSampleFactory<PlanTemplateDTO>
    {
        private readonly ISwaggerSampleFactory<PlanNodeDescriptionDTO> _planNodeDescriptionSampleFactory;
        public PlanTemplateSampleFactory(ISwaggerSampleFactory<PlanNodeDescriptionDTO> planNodeDescriptionSampleFactory)
        {
            _planNodeDescriptionSampleFactory = planNodeDescriptionSampleFactory;
        }

        public PlanTemplateDTO GetSampleData()
        {
            return new PlanTemplateDTO
            {
                Id = Guid.Parse("8A95B9C1-D6F9-4B8C-A603-EFED0F1B15AB"),
                Name = "Plan Name",
                Description = "Plan Description",
                PlanNodeDescriptions = new List<PlanNodeDescriptionDTO> {  _planNodeDescriptionSampleFactory.GetSampleData() },
                StartingPlanNodeDescriptionId = _planNodeDescriptionSampleFactory.GetSampleData().Id
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}