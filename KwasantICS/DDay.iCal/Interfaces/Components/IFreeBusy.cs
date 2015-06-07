using System.Collections.Generic;
using KwasantICS.DDay.iCal.Interfaces.DataTypes;
using KwasantICS.DDay.iCal.Interfaces.General;

namespace KwasantICS.DDay.iCal.Interfaces.Components
{
    public interface IFreeBusy :
        IUniqueComponent,
        IMergeable
    {
        IList<IFreeBusyEntry> Entries { get; set; }
        IDateTime DTStart { get; set; }
        IDateTime DTEnd { get; set; }
        IDateTime Start { get; set; }
        IDateTime End { get; set; }

        FreeBusyStatus GetFreeBusyStatus(IPeriod period);
        FreeBusyStatus GetFreeBusyStatus(IDateTime dt);
    }
}
