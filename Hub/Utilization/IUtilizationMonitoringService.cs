using Data.Entities;

namespace Hub.Utilization
{
    public interface IUtilizationMonitoringService
    {
        int AggregationUnitDuration { get; }
        void TrackActivityExecution(ActivityDO activity, ContainerDO container);
    }
}