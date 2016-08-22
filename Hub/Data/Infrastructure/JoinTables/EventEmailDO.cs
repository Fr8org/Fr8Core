using System.ComponentModel.DataAnnotations;

namespace Data.Infrastructure.JoinTables
{
    public class EventEmailDO
    {
        [Key]
        public int EventID { get; set; }
        [Key]
        public int EmailID { get; set; }
    }
}
