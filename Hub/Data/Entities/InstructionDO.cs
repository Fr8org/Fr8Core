using System;
using System.ComponentModel.DataAnnotations;
namespace Data.Entities
{
    public class InstructionDO : BaseObject
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public String Category { get; set; }
    }
}
