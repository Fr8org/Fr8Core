using Data.Infrastructure;
using Data.States;

namespace HubWeb.NotificationQueues
{
    public class HighPriorityIncidentsQueue : SharedNotificationQueue<HighPriorityIncidentData>
    {
        public HighPriorityIncidentsQueue()
        {
            EventManager.AlertHighPriorityIncidentCreated += id => AppendUpdate(new HighPriorityIncidentData(id));
        }
    }

    public class HighPriorityIncidentData : IRoleUpdateData
    {
        public HighPriorityIncidentData(int incidentId)
        {
            IncidentId = incidentId;
        }

        public int IncidentId { get; private set; }
        public string[] RoleNames { get { return new[] { Roles.Admin }; } }
    }
}