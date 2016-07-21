using Data.Entities;

namespace Hub.Interfaces
{
    public interface IUtilizationMonitoringService
    {
        int AggregationUnitDuration { get; }
        void TrackActivityExecution(ActivityDO activity, ContainerDO container);
    }
}