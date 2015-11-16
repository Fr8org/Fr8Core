using System;
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
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int RouteId { get; set; }
        public int ContainerState;

        public int? CurrentRouteNodeId { get; set; }
        public int? NextRouteNodeId { get; set; }

        public string LastUpdated { get; set; }

        public string CreateDate { get; set; }

    }
}
