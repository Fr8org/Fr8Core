using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using StructureMap;
using Data.Interfaces;
using Data.Infrastructure;
using Data.Interfaces.DataTransferObjects;

namespace Data.Entities
{
    public class RouteNodeDO : BaseObject
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("RootRouteNode")]
        public Guid? RootRouteNodeId { get; set; }

        public virtual RouteNodeDO RootRouteNode { get; set; }

        [ForeignKey("ParentRouteNode")]
        public Guid? ParentRouteNodeId { get; set; }
        
        public virtual RouteNodeDO ParentRouteNode { get; set; }

        [InverseProperty("ParentRouteNode")]
        public virtual IList<RouteNodeDO> ChildNodes { get; set; }


        [ForeignKey("Fr8Account")]
        public string Fr8AccountId { get; set; }

        public virtual Fr8AccountDO Fr8Account { get; set; }

        public int Ordering { get; set; }


        public RouteNodeDO()
        {
            ChildNodes = new List<RouteNodeDO>();
        }

        public virtual RouteNodeDO Clone()
        {
            return new RouteNodeDO()
            {
                Ordering = this.Ordering
            };
        }
    }
}