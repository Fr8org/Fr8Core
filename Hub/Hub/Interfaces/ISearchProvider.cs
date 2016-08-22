using System;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects.PlanDirectory;
using Fr8.Infrastructure.Data.Manifests;

namespace Hub.Interfaces
{
    public interface ISearchProvider
    {
        Task Initialize(bool recreate);
        Task<SearchResultDTO> Search(SearchRequestDTO request);
        Task CreateOrUpdate(PlanTemplateCM planTemplate);
        Task Remove(Guid planId);
    }
}
