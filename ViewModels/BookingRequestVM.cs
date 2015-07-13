using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.ViewModels
{
    public class BookingRequestsVM
    {
        public BookingRequestsVM()
        {
            BookingRequests = new List<BookingRequestVM>();
        }

        public List<BookingRequestVM> BookingRequests { get; set; }
    }

    public class BookingRequestVM
    {
        public int Id { get; set; }
        public String BookerName { get; set; } 
        public String Subject { get; set; }
        public String EmailAddress { get; set; }
        public DateTimeOffset DateReceived { get; set; }
        public String HTMLText { get; set; }
    }

}