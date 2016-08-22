using System;
using System.Collections.Generic;
using System.Reflection;
using Data.Entities;

namespace Data.Repositories.Plan
{
    public class PlanSnapshot
    {
        /**********************************************************************************/
        // Nested
        /**********************************************************************************/

        public class ChangedObject
        {
            public readonly List<PropertyInfo> ChangedProperties;
            public readonly PlanNodeDO Node;

            public ChangedObject(PlanNodeDO node, List<PropertyInfo> changedProperties)
            {
                Node = node;
                ChangedProperties = changedProperties;
            }
        }

        /**********************************************************************************/

        public class Changes
        {
            public readonly List<PlanNodeDO> Insert = new List<PlanNodeDO>();
            public readonly List<PlanNodeDO> Delete = new List<PlanNodeDO>();
            public readonly List<ChangedObject> Update = new List<ChangedObject>();

            public bool HasChanges
            {
                get { return Insert.Count > 0 || Delete.Count > 0 || Update.Count > 0; }
            }
        }

        /**********************************************************************************/
        // Declarations
        /**********************************************************************************/

        private readonly Dictionary<Guid, PlanNodeDO> _nodes = new Dictionary<Guid, PlanNodeDO>();

        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

        public PlanSnapshot()
        {
        }

        /**********************************************************************************/

        public PlanSnapshot(PlanNodeDO node, bool cloneNodes)
        {
            PlanTreeHelper.Visit(node, (x, y) =>
            {
                var clone = cloneNodes ? x.Clone() : x;

                _nodes[x.Id] = clone;

                clone.ParentPlanNodeId = y != null ? y.Id : (Guid?)null;
            });
        }

        /**********************************************************************************/

        public Changes Compare(PlanSnapshot referece)
        {
            var diff = new Changes();
            List<PropertyInfo> changedProperties = null;

            PlanNodeDO node;

            foreach (var refNodeId in _nodes)
            {
                if (!referece._nodes.TryGetValue(refNodeId.Key, out node))
                {
                    diff.Insert.Add(refNodeId.Value);
                }
                else
                {
                    if (changedProperties == null)
                    {
                        changedProperties = new List<PropertyInfo>();
                    }

                    node.CheckModified(refNodeId.Value, changedProperties);

                    if (changedProperties.Count > 0)
                    {
                        diff.Update.Add(new ChangedObject(refNodeId.Value, changedProperties));
                        changedProperties = null;
                    }
                }
            }

            foreach (var refNodeId in referece._nodes)
            {

                if (!_nodes.TryGetValue(refNodeId.Key, out node))
                {
                    diff.Delete.Add(refNodeId.Value);
                }
            }

            return diff;
        }

        /**********************************************************************************/
    }
}