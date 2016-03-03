using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.States;

namespace Data.Repositories.Plan
{
    public class PlanCache : IPlanCache
    {
        /**********************************************************************************/

        private class CachedPlan
        {
            public PlanNodeDO Root { get; private set; }
            public IExpirationToken Expiration { get; set; }

            public CachedPlan(PlanNodeDO root, IExpirationToken expiration)
            {
                Root = root;
                Expiration = expiration;
            }
        }

        /**********************************************************************************/

        private class CacheItem
        {
            public readonly PlanNodeDO Node;
            public readonly CachedPlan Plan;

            public CacheItem(PlanNodeDO node, CachedPlan plan)
            {
                Node = node;
                Plan = plan;
            }
        }

        /**********************************************************************************/
        // Declarations
        /**********************************************************************************/
        
        private readonly Dictionary<Guid, CacheItem> _planNodesLookup = new Dictionary<Guid, CacheItem>();
        private readonly Dictionary<Guid, CachedPlan> _plans = new Dictionary<Guid, CachedPlan>();
        private readonly object _sync = new object();
        private readonly IPlanCacheExpirationStrategy _expirationStrategy;
        
        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

        public PlanCache(IPlanCacheExpirationStrategy expirationStrategy)
        {
            _expirationStrategy = expirationStrategy;
            expirationStrategy.SetExpirationCallback(RemoveExpiredPlans);
        }
        
        /**********************************************************************************/

        public PlanNodeDO Get(Guid id, Func<Guid, PlanNodeDO> cacheMissCallback)
        {
            PlanNodeDO node;

            lock (_sync)
            {
                CacheItem cacheItem;

                if (!_planNodesLookup.TryGetValue(id, out cacheItem))
                {
                    node = cacheMissCallback(id);

                    if (node == null)
                    {
                        return null;
                    }

                    // Get the root of PlanNode tree. 
                    while (node.ParentPlanNode != null)
                    {
                        node = node.ParentPlanNode;
                    }

                    // Check cache integrity
                    if (PlanTreeHelper.Linearize(node).Any(x => _planNodesLookup.ContainsKey(x.Id)))
                    {
                        DropCachedPlan(node);
                    }

                    AddToCache(node);
                }
                else
                {
                    node = cacheItem.Plan.Root;
                    // update plan expiration
                    cacheItem.Plan.Expiration = _expirationStrategy.NewExpirationToken();
                }
            }

            return node;
        }

        /**********************************************************************************/
        
        public void UpdateElements(Action<PlanNodeDO> updater)
        {
            lock (_sync)
            {
                foreach (var cacheItem in _planNodesLookup.Values)
                {
                    updater(cacheItem.Node);
                }
            }
        }

        /**********************************************************************************/

        public void UpdateElement(Guid id, Action<PlanNodeDO> updater)
        {
            lock (_sync)
            {
                CacheItem node;

                if (_planNodesLookup.TryGetValue(id, out node))
                {
                    updater(node.Node);
                }
            }
        }

        /**********************************************************************************/

        public void Update(Guid planId, PlanSnapshot.Changes changes)
        {
            lock (_sync)
            {
                CachedPlan plan;

                if (!_plans.TryGetValue(planId, out plan))
                {
                    foreach (var insert in changes.Insert)
                    {
                        var clone = insert.Clone();

                        if (insert is PlanDO)
                        {
                            plan = new CachedPlan((PlanDO) clone, _expirationStrategy.NewExpirationToken());
                            _plans.Add(planId, plan);
                            _planNodesLookup.Add(planId, new CacheItem(clone, plan));
                            clone.RootPlanNode = clone;
                            break;
                        }
                    }
                }

                foreach (var insert in changes.Insert)
                {
                    if (!_planNodesLookup.ContainsKey(insert.Id))
                    {
                        _planNodesLookup.Add(insert.Id, new CacheItem(insert.Clone(), plan));
                    }
                }

                foreach (var insert in changes.Insert)
                {
                    var node = _planNodesLookup[insert.Id].Node;

                    if (insert.ParentPlanNodeId != null)
                    {
                        var parent = _planNodesLookup[insert.ParentPlanNodeId.Value].Node;

                        parent.ChildNodes.Add(node);
                        node.ParentPlanNode = parent;
                        node.RootPlanNode = plan.Root;
                    }
                }

                foreach (var deleted in changes.Delete)
                {
                    CachedPlan cachedPlan;

                    if (_plans.TryGetValue(deleted.Id, out cachedPlan))
                    {
                        PlanTreeHelper.Visit(cachedPlan.Root, x => _planNodesLookup.Remove(x.Id));
                        _plans.Remove(cachedPlan.Root.Id);
                        return;
                    }

                    CacheItem node;
                    if (_planNodesLookup.TryGetValue(deleted.Id, out node))
                    {
                        _planNodesLookup.Remove(deleted.Id);
                        node.Node.RemoveFromParent();
                    }
                }

                foreach (var update in changes.Update)
                {
                    foreach (var changedProperty in update.ChangedProperties)
                    {
                        var original = _planNodesLookup[update.Node.Id].Node;

                        // structure was changed
                        if (changedProperty.Name == "ParentPlanNodeId")
                        {
                            var parent = _planNodesLookup[update.Node.ParentPlanNodeId.Value].Node;

                            original.RemoveFromParent();
                            parent.ChildNodes.Add(original);
                            original.ParentPlanNode = parent;
                        }
                        else
                        {
                            changedProperty.SetValue(original, changedProperty.GetValue(update.Node));
                        }
                    }
                }
            }
        }

        /**********************************************************************************/

        private void AddToCache(PlanNodeDO root)
        {
            var expirOn = _expirationStrategy.NewExpirationToken();
            var cachedPlan = new CachedPlan(root, expirOn);
            _plans.Add(root.Id, cachedPlan);

            PlanTreeHelper.Visit(root, x => _planNodesLookup.Add(x.Id, new CacheItem(x, cachedPlan)));
        }

        /**********************************************************************************/

        private void DropCachedPlan(PlanNodeDO root)
        {
            CachedPlan cachedPlan;
            
            if (!_plans.TryGetValue(root.Id, out cachedPlan))
            {
                return;
            }

            PlanTreeHelper.Visit(root, x => _planNodesLookup.Remove(x.Id));
            
            _plans.Remove(root.Id);
        }

        /**********************************************************************************/

        private void RemoveExpiredPlans()
        {
            lock (_sync)
            {
                foreach (var planExpiration in _plans.ToArray())
                {
                    if (planExpiration.Value.Expiration.IsExpired())
                    {
                        _plans.Remove(planExpiration.Key);
                        PlanTreeHelper.Visit(planExpiration.Value.Root, x => _planNodesLookup.Remove(x.Id));
                    }
                }
            }
        }

        /**********************************************************************************/
    }
}