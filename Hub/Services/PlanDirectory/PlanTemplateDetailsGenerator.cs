using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Hub.Interfaces;

namespace HubWeb.Infrastructure_PD.Infrastructure
{
    public class PlanTemplateDetailsGenerator : IPlanTemplateDetailsGenerator
    {
        private readonly ITemplateGenerator _templateGenerator;

        public PlanTemplateDetailsGenerator(ITemplateGenerator templateGenerator)
        {
            if (templateGenerator == null)
                throw new ArgumentNullException(nameof(templateGenerator));

            _templateGenerator = templateGenerator;
        }

        public async Task Generate(PublishPlanTemplateDTO publishPlanTemplateDto)
        {
            var pageName = publishPlanTemplateDto.Name + ".html";
            if (publishPlanTemplateDto.Description == null)
                publishPlanTemplateDto.Description = publishPlanTemplateDto.Name;
            await _templateGenerator.Generate(new PlanTemplateDetailsTemplate(), pageName, new Dictionary<string, object>
            {
                ["planTemplate"] = publishPlanTemplateDto
            });
        }
    }
}