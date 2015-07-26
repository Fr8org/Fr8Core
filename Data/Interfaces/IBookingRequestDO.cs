using System.ComponentModel.DataAnnotations;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IBookingRequestDO : IEmailDO
    {
        [Required]
        DockyardAccountDO User { get; set; }

      
    }
}