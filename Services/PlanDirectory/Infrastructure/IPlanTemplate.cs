using System.Collections.Generic;
using System.Threading.Tasks;
using PlanDirectory.Interfaces;

namespace PlanDirectory.Infrastructure
{
    public interface IPlanTemplate
    {
        Task Initialize();

        Task CreateOrUpdate(string fr8AccountId, PublishPlanTemplateDTO planTemplate);

        // TODO: FR-3539, fix this.
        // Task<SearchResultDTO> Search(SearchRequestDTO request);

        // TODO: FR-3539, remove this.
        // Task Publish(PublishPlanTemplateDTO planTemplate);
        // Task Unpublish(PublishPlanTemplateDTO planTemplate);
    }
}
