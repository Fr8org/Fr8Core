using System.Threading.Tasks;
using PlanDirectory.Interfaces;

namespace PlanDirectory.Infrastructure
{
    public interface ISearchProvider
    {
        Task Initialize(bool recreate);
        Task<SearchResultDTO> Search(SearchRequestDTO request);
        Task CreateOrUpdate(PlanTemplateDTO planTemplate);
    }
}
