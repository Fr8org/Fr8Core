using System;
using System.Threading.Tasks;
using Fr8Data.Manifests;
using PlanDirectory.Interfaces;

namespace PlanDirectory.Infrastructure
{
    public interface ISearchProvider
    {
        Task Initialize(bool recreate);
        Task<SearchResultDTO> Search(SearchRequestDTO request);
        Task CreateOrUpdate(PlanTemplateCM planTemplate);
        Task Remove(Guid planId);
    }
}
