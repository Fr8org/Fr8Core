using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Entities;

namespace Data.Interfaces.DataTransferObjects
{
    public class ContainerDTO
    {
        public ContainerDTO()
        {
        }

        [Required]
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProcessTemplateId { get; set; }
        public int ContainerState;

        public int? CurrentActivityId { get; set; }
        public int? NextActivityId { get; set; }

    }
}
