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
        public ManifestRegistryMonitorResult(Guid planId, bool isNewPlan = true)
        {
            PlanId = planId;
            IsNewPlan = isNewPlan;
        }
        /// <summary>
        /// Gets Id of monitoring plan
        /// </summary>
        public Guid PlanId { get; private set; }
        /// <summary>
        /// Gets a value indicating whether new plan was created or existing one was used
        /// </summary>
        public bool IsNewPlan { get; private set; }
    }
}
