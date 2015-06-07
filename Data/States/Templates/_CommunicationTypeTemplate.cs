using System;
using System.ComponentModel.DataAnnotations;

namespace Data.States.Templates
{
    public class _CommunicationTypeTemplate
    {
        [Key]
        public int Id { get; set; }
        public String Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
