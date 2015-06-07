using System.Collections.Generic;
using KwasantICS.DDay.iCal.Interfaces.Evaluation;

namespace KwasantICS.DDay.iCal.Interfaces
{
    public interface IICalendarCollection :
        IGetOccurrencesTyped,
        IList<IICalendar>
    {        
    }
}
