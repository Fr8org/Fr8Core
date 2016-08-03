using System;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Manifests;

namespace HubWeb.Infrastructure_PD.Interfaces
{
    public interface ISearchProvider
    {
        Task Initialize(bool recreate);
        Task<SearchResultDTO> Search(SearchRequestDTO request);
        Task CreateOrUpdate(PlanTemplateCM planTemplate);
        Task Remove(Guid planId);
    }
}
