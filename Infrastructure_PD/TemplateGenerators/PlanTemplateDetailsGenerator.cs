using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities.Configuration;
using Hub.Interfaces;
using HubWeb.Templates;

namespace HubWeb.Infrastructure_PD.TemplateGenerators
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
            var pageName = publishPlanTemplateDto.Name + "-" + publishPlanTemplateDto.ParentPlanId + ".html";
            if (publishPlanTemplateDto.Description == null)
                publishPlanTemplateDto.Description = "";

            await _templateGenerator.Generate(new PlanTemplateDetailsTemplate(), pageName, new Dictionary<string, object>
            {
                ["planTemplate"] = publishPlanTemplateDto,
                ["planCreateUrl"] = CloudConfigurationManager.GetSetting("HubApiUrl") +
                "plan_templates/createplan/?id=" + publishPlanTemplateDto.ParentPlanId
            });
        }

        public Task<bool> HasGeneratedPage(PublishPlanTemplateDTO planTemplate)
        {
            var pageName = $"{planTemplate.Name}-{planTemplate.ParentPlanId.ToString()}.html";
            return Task.FromResult(File.Exists(Path.Combine(_templateGenerator.OutputFolder, pageName)));
        }
    }
}