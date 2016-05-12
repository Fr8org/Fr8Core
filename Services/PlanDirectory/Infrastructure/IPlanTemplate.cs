using System.Collections.Generic;
using System.Threading.Tasks;
using PlanDirectory.Interfaces;

namespace PlanDirectory.Infrastructure
{
    public interface IPlanTemplate
    {
        Task Initialize();

        Task<SearchResultDTO> Search(SearchRequestDTO request);

        Task Publish(PublishPlanTemplateDTO planTemplate);
        Task Unpublish(PublishPlanTemplateDTO planTemplate);
    }
}
