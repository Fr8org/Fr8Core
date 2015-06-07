using System.ComponentModel.DataAnnotations;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IBookingRequestDO : IEmailDO
    {
        [Required]
        UserDO User { get; set; }

      
    }
}