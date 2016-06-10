using System;
using System.Threading.Tasks;

namespace Hub.Interfaces
{
    public interface IManifestRegistryMonitor
    {
        Task<ManifestRegistryMonitorResult> StartMonitoringManifestRegistrySubmissions();
    }

    public class ManifestRegistryMonitorResult
    {
        public ManifestRegistryMonitorResult(Guid planId, bool isNewPlan)
        {
            PlanId = planId;
            IsNewPlan = isNewPlan;
        }

        public Guid PlanId { get; private set; }

        public bool IsNewPlan { get; private set; }
    }
}
