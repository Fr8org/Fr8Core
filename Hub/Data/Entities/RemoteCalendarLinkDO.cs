using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;

namespace Data.Entities
{
    /// <summary>
    /// A link between local Kwasant user's calendar and his calendar on a remote calendar provider.
    /// Also it is a tracking record for synchronization progress.
    /// </summary>
    public class RemoteCalendarLinkDO : BaseDO, IRemoteCalendarLinkDO
    {
        [NotMapped]
        ICalendarDO IRemoteCalendarLinkDO.LocalCalendar
        {
            get { return LocalCalendar; }
            set { LocalCalendar = (CalendarDO)value; }
        }

        [NotMapped]
        IRemoteCalendarProviderDO IRemoteCalendarLinkDO.Provider
        {
            get { return Provider; }
            set { Provider = (RemoteCalendarProviderDO)value; }
        }

        [Key]
        public int Id { get; set; }
        
        public string RemoteCalendarHref { get; set; }
        public string RemoteCalendarName { get; set; }
        public bool IsDisabled { get; set; }

        [Required, ForeignKey("LocalCalendar")]
        public int? LocalCalendarID { get; set; }
        public virtual CalendarDO LocalCalendar { get; set; }
        
        [Required, ForeignKey("Provider")]
        public int? ProviderID { get; set; }
        public virtual RemoteCalendarProviderDO Provider { get; set; }

        public DateTimeOffset DateSynchronizationAttempted { get; set; }
        public DateTimeOffset DateSynchronized { get; set; }
        public string LastSynchronizationResult { get; set; }
    }
}