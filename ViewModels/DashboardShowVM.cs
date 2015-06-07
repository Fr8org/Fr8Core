using System.Collections.Generic;

namespace KwasantWeb.ViewModels
{
    public class DashboardShowVM
    {
        public BookingRequestAdminVM BookingRequestVM { get; set; }
        public CalendarShowVM CalendarVM { get; set; }
        public List<DashboardNegotiationVM> ResolvedNegotiations { get; set; }
    }

    public class DashboardNegotiationVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}