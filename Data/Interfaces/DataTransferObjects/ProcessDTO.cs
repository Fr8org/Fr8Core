using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Entities;
using Data.States;

namespace Data.Interfaces.DataTransferObjects
{
    public class ProcessDTO
    {
        public ProcessDTO()
        {
        }

        [Required]
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProcessTemplateId { get; set; }
        public int processState;

        public int? CurrentActivityId { get; set; }
        public int? NextActivityId { get; set; }

    }
}
