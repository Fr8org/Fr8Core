using System;
using System.Collections.Generic;

namespace KwasantWeb.Controllers.External.DayPilot.Providers
{
    public interface IEventDataProvider
    {
        List<DayPilotTimeslotInfo> LoadData();
        String GetTimeslotBubbleText(int id);

        void BeforeCellRender(global::DayPilot.Web.Mvc.Events.Calendar.BeforeEventRenderArgs e);
        void BeforeCellRender(global::DayPilot.Web.Mvc.Events.Month.BeforeEventRenderArgs e);
    }

    public class DayPilotTimeslotInfo
    {
        public int? CalendarID { get; set; }
        public bool IsAllDay { get; set; }
        public String StartDate { get; set; }
        public String EndDate { get; set; }
        public String Text { get; set; }
        public String Tag { get; set; }
        public int Id { get; set; }
        public String GroupingID { get; set; }
    }
}
