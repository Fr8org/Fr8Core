using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.Manifests;

namespace Hub.Interfaces
{
    public interface IUpstreamDataExtractionService
    {
        void ExtactAndAssignValues(StandardConfigurationControlsCM configurationControls, ICrateStorage payloadStorage);
    }
}
