using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Web.ViewModels
{
    public class BookingRequestNoteVM
    {
        public int BookingRequestId { get; set; }
        public string Note { get; set; }
    }
}