using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Data.Infrastructure.StructureMap;
using Data.Repositories.Plan;
using Data.States;
using StructureMap;

namespace Data.Entities
{
    public class PlanNodeDO : BaseObject
    {
        /*
         * IMPORTANT: IF YOU'RE ADDING A NEW PROPERTY/FIELD, 
         * be sure to declare it in the following places, otherwise values 
         * of the new properties will not be persisted:
         * 
         * 1. Add it to the list of tracked properties, e.g. 
         *      typeof(PlanNodeDO).GetProperty(nameof(MyNewProperty))
         *    Note: don't add virtual navigation properties to this list, 
         *    only add the foreign key property for a navigation property. 
         * 
         * 2. Add it to the CopyProperties() method, e.g.
         *      MyNewProperty = plan.MyNewProperty;
         *      
         */

        private static readonly PropertyInfo[] TrackingProperties =
        {
            typeof (PlanNodeDO).GetProperty(nameof(ParentPlanNodeId)),
            typeof (PlanNodeDO).GetProperty(nameof(Fr8AccountId)),
            typeof (PlanNodeDO).GetProperty(nameof(Ordering)),
            typeof (PlanNodeDO).GetProperty(nameof(LastUpdated))
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

        public PlanNodeDO()
        {
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

            var securityService = ObjectFactory.GetInstance<ISecurityServices>();
            securityService.SetDefaultRecordBasedSecurityForObject(Roles.OwnerOfCurrentObject, Id, nameof(PlanNodeDO));
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
        }
    }
}