using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.States.Templates
{
    public class _BookingRequestAvailabilityTemplate : IStateTemplate<BookingRequestAvailability>
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public String Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
