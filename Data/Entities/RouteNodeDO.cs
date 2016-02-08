using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Data.Repositories.Plan;

namespace Data.Entities
{
    public class RouteNodeDO : BaseObject
    {
        private static readonly PropertyInfo[] TrackingProperties =
        {
            typeof (RouteNodeDO).GetProperty("ParentRouteNodeId"),
            typeof (RouteNodeDO).GetProperty("Fr8AccountId"),
            typeof (RouteNodeDO).GetProperty("Ordering"),
        };

        [Key]
        public Guid Id { get; set; }

        [ForeignKey("RootRouteNode")]
        public Guid? RootRouteNodeId { get; set; }

        public RouteNodeDO RootRouteNode { get; set; }

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

        public void AddChild(RouteNodeDO child, int? ordering)
        {
            child.Ordering = ordering ?? (ChildNodes.Count > 0 ? ChildNodes.Max(x => x.Ordering) + 1 : 1);
            ChildNodes.Add(child);
        }

        public virtual RouteNodeDO Clone()
        {
            var clone = CreateNewInstance();
            clone.CopyProperties(this);
            return clone;
        }

        public void SyncPropertiesWith(RouteNodeDO other)
        {
            CopyProperties(other);
        }

        public virtual bool AreContentPropertiesEquals(RouteNodeDO other)
        {
            return ParentRouteNodeId == other.ParentRouteNodeId &&
                   Fr8AccountId == other.Fr8AccountId &&
                   Fr8Account == other.Fr8Account &&
                   Ordering == other.Ordering;
        }

        public List<RouteNodeDO> GetDescendants()
        {
            return RouteTreeHelper.Linearize(this);
        }

        public List<RouteNodeDO> GetOrderedChildren()
        {
            return ChildNodes.OrderBy(x => x.Ordering).ToList();
        }

        public RouteNodeDO GetTreeRoot()
        {
            var node = this;

            while (node.ParentRouteNode != null)
            {
                node = node.ParentRouteNode;
            }

            return node;
        }

        public List<RouteNodeDO> GetDescendantsOrdered()
        {
            return RouteTreeHelper.LinearizeOrdered(this);
        }

        protected virtual RouteNodeDO CreateNewInstance()
        {
            return new RouteNodeDO();
        }

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