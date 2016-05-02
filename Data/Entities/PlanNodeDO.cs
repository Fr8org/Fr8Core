using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Data.Infrastructure.StructureMap;
using Data.Repositories.Plan;
using StructureMap;

namespace Data.Entities
{
    public class PlanNodeDO : BaseObject
    {
        private static readonly PropertyInfo[] TrackingProperties =
        {
            typeof (PlanNodeDO).GetProperty(nameof(ParentPlanNodeId)),
            typeof (PlanNodeDO).GetProperty(nameof(Fr8AccountId)),
            typeof (PlanNodeDO).GetProperty(nameof(Ordering)),
            typeof (PlanNodeDO).GetProperty(nameof(LastUpdated)),
            typeof (PlanNodeDO).GetProperty(nameof(Runnable))
        };

        [Key]
        public Guid Id { get; set; }

        [ForeignKey("RootPlanNode")]
        public Guid? RootPlanNodeId { get; set; }

        public PlanNodeDO RootPlanNode { get; set; }

        [ForeignKey("ParentPlanNode")]
        public Guid? ParentPlanNodeId { get; set; }

        public virtual PlanNodeDO ParentPlanNode { get; set; }

        [InverseProperty("ParentPlanNode")]
        public virtual IList<PlanNodeDO> ChildNodes { get; set; }

        [ForeignKey("Fr8Account")]
        public string Fr8AccountId { get; set; }

        public virtual Fr8AccountDO Fr8Account { get; set; }

        public int Ordering { get; set; }

        /// <summary>
        /// Flag to indicate whether to execute current PlanNode during run-time or not.
        /// Specifically when working with subordinate subplans,
        /// we do not want to execute a subplan that was created during design-time mode,
        /// since such subplan only provides template data for downstream activities during design-time. (FR-2908).
        /// </summary>
        public bool Runnable { get; set; }

        public PlanNodeDO()
        {
            Runnable = true;
            ChildNodes = new List<PlanNodeDO>();
        }

        public void RemoveFromParent()
        {
            if (ParentPlanNode != null)
            {
                ParentPlanNode.ChildNodes.Remove(this);
            }
        }

        public void AddChildWithDefaultOrdering(PlanNodeDO child)
        {
            child.Ordering = ChildNodes.Count > 0 ? ChildNodes.Max(x => x.Ordering) + 1 : 1;
            ChildNodes.Add(child);
        }

        public void AddChild(PlanNodeDO child, int? ordering)
        {
            child.Ordering = ordering ?? (ChildNodes.Count > 0 ? ChildNodes.Max(x => x.Ordering) + 1 : 1);
            ChildNodes.Add(child);
        }

        public virtual PlanNodeDO Clone()
        {
            var clone = CreateNewInstance();
            clone.CopyProperties(this);
            return clone;
        }

        public void SyncPropertiesWith(PlanNodeDO other)
        {
            CopyProperties(other);
        }

        public virtual bool AreContentPropertiesEquals(PlanNodeDO other)
        {
            return ParentPlanNodeId == other.ParentPlanNodeId &&
                   Fr8AccountId == other.Fr8AccountId &&
                   Fr8Account == other.Fr8Account &&
                   Ordering == other.Ordering;
        }

        public List<PlanNodeDO> GetDescendants()
        {
            return PlanTreeHelper.Linearize(this);
        }

        public List<PlanNodeDO> GetOrderedChildren()
        {
            return ChildNodes.OrderBy(x => x.Ordering).ToList();
        }

        public PlanNodeDO GetTreeRoot()
        {
            var node = this;

            while (node.ParentPlanNode != null)
            {
                node = node.ParentPlanNode;
            }

            return node;
        }

        public override void AfterCreate()
        {
            base.AfterCreate();

            //var securityService = ObjectFactory.GetInstance<ISecurityServices>();
            //securityService.SetDefaultObjectSecurity(Id, GetType().Name);
        }

        public List<PlanNodeDO> GetDescendantsOrdered()
        {
            return PlanTreeHelper.LinearizeOrdered(this);
        }

        protected virtual PlanNodeDO CreateNewInstance()
        {
            return new PlanNodeDO();
        }

        protected virtual IEnumerable<PropertyInfo> GetTrackingProperties()
        {
            return TrackingProperties;
        }

        public virtual void CheckModified(PlanNodeDO other, List<PropertyInfo> changedProperties)
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

        protected virtual void CopyProperties(PlanNodeDO source)
        {
            Id = source.Id;
            ParentPlanNodeId = source.ParentPlanNodeId;
            Fr8AccountId = source.Fr8AccountId;
            Fr8Account = source.Fr8Account;
            RootPlanNodeId = source.RootPlanNodeId;
            Ordering = source.Ordering;
            Runnable = source.Runnable;
        }
    }
}