using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IDockyardAccountDO : IBaseDO
    {
        string Id { get; set; }
        //IList<BookingRequestDO> UserBookingRequests { get; set; }
        IEmailAddressDO EmailAddress { get; }
        IList<RemoteCalendarAuthDataDO> RemoteCalendarAuthData { get; set; }
        IList<ISubscriptionDO> Subscriptions { get; set; } 
        bool IsRemoteCalendarAccessGranted(string providerName);
    }
}
