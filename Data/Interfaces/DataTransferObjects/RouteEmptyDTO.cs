using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Entities;

namespace Data.Interfaces.DataTransferObjects
{
    public class RouteEmptyDTO
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Tag { get; set; }

        public string Description { get; set; }

        public int RouteState { get; set; }

        public int StartingSubrouteId { get; set; }
    }
}