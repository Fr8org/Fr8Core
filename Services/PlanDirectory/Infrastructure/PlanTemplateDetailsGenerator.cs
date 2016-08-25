using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Data.DataTransferObjects;
using PlanDirectory.Interfaces;
using PlanDirectory.Templates;

namespace PlanDirectory.Infrastructure
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
            await _templateGenerator.Generate(new PlanTemplateDetailsTemplate(), pageName, new Dictionary<string, object>
            {
                ["planTemplate"] = publishPlanTemplateDto
            });
        }
    }
}