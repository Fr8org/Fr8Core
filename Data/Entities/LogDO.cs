using System;
using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
    public class LogDO : BaseObject
    {
        [Key]
        public int Id { get; set; }
        public String Message { get; set; }
        public String Name { get; set; }
        public String Level { get; set; }
    }
}
