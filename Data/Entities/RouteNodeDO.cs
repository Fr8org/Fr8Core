using Data.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Infrastructure;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using StructureMap;

namespace Data.Entities
{
    public class RouteNodeDO : BaseDO
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ParentRouteNode")]
        public int? ParentRouteNodeId { get; set; }
        
        public virtual RouteNodeDO ParentRouteNode { get; set; }

        [InverseProperty("ParentRouteNode")]
        public virtual IList<RouteNodeDO> RouteNodes { get; set; }

        public int Ordering { get; set; }


        public RouteNodeDO()
        {
            RouteNodes = new List<RouteNodeDO>();
        }
    }
}