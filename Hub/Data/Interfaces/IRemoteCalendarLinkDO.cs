using System;

namespace Data.Interfaces
{
    public interface IRemoteCalendarLinkDO : IBaseDO
    {
        int Id { get; set; }
        string RemoteCalendarHref { get; set; }
        string RemoteCalendarName { get; set; }
        bool IsDisabled { get; set; }

        int? LocalCalendarID { get; set; }
        ICalendarDO LocalCalendar { get; set; }

        int? ProviderID { get; set; }
        IRemoteCalendarProviderDO Provider { get; set; }

        DateTimeOffset DateSynchronizationAttempted { get; set; }
        DateTimeOffset DateSynchronized { get; set; }
        string LastSynchronizationResult { get; set; }
    }
}