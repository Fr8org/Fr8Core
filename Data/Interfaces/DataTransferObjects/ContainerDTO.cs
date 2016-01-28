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
        public Guid RouteId { get; set; }
        public int ContainerState;

        public Guid? CurrentRouteNodeId { get; set; }
        public Guid? NextRouteNodeId { get; set; }

        public DateTimeOffset LastUpdated { get; set; }

        public DateTimeOffset CreateDate { get; set; }

    }
}
