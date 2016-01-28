using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
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

        public void RemoveFromParent()
        {
            if (ParentRouteNode != null)
            {
                ParentRouteNode.ChildNodes.Remove(this);
            }
        }

        public void AddChildWithDefaultOrdering(RouteNodeDO child)
        {
            child.Ordering = ChildNodes.Count > 0 ? ChildNodes.Max(x => x.Ordering) + 1 : 1;
            ChildNodes.Add(child);
        }

        public virtual RouteNodeDO Clone()
        {
            var clone = CreateNewInstance();
            clone.CopyProperties(this);
            return clone;
        }

        public virtual bool AreContentPropertiesEquals(RouteNodeDO other)
        {
            return ParentRouteNodeId == other.ParentRouteNodeId &&
                   Fr8AccountId == other.Fr8AccountId &&
                   Fr8Account == other.Fr8Account &&
                   Ordering == other.Ordering;
        }

        protected virtual RouteNodeDO CreateNewInstance()
        {
            return new RouteNodeDO();
        }

        private static readonly PropertyInfo[] TrackingProperties = 
        {
            typeof(RouteNodeDO).GetProperty("ParentRouteNodeId"),
            typeof(RouteNodeDO).GetProperty("Fr8AccountId"),
            typeof(RouteNodeDO).GetProperty("Ordering"),
        };

        protected virtual IEnumerable<PropertyInfo> GetTrackingProperties()
        {
            return TrackingProperties;
        }

        public virtual void CheckModified(RouteNodeDO other, List<PropertyInfo> changedProperties)
        {
            foreach (var trackingProperty in GetTrackingProperties())
            {
                var current = trackingProperty.GetValue(this);
                var otherValue = trackingProperty.GetValue(other);
                
                if (!Equals(current, otherValue))
                {
                    changedProperties.Add(trackingProperty);
                }
            }
        }

        protected virtual void CopyProperties(RouteNodeDO source)
        {
            Id = source.Id;
            ParentRouteNodeId = source.ParentRouteNodeId;
            Fr8AccountId = source.Fr8AccountId;
            Fr8Account = source.Fr8Account;
            RootRouteNodeId = source.RootRouteNodeId;
            Ordering = source.Ordering;
        }
    }
}