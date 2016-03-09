using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.States;
using static Data.Repositories.Plan.RouteSnapshot;

namespace Data.Repositories.Plan
{
    public class PlanCache : IPlanCache
    {
        /**********************************************************************************/

        private class CachedRoute
        {
            public RouteNodeDO Root { get; private set; }
            public IExpirationToken Expiration { get; set; }

            public CachedRoute(RouteNodeDO root, IExpirationToken expiration)
            {
                Root = root;
                Expiration = expiration;
            }
        }

        /**********************************************************************************/

        private class CacheItem
        {
            public readonly RouteNodeDO Node;
            public readonly CachedRoute Route;

            public CacheItem(RouteNodeDO node, CachedRoute route)
            {
                Node = node;
                Route = route;
            }
        }

        /**********************************************************************************/
        // Declarations
        /**********************************************************************************/
        
        private readonly Dictionary<Guid, CacheItem> _routeNodesLookup = new Dictionary<Guid, CacheItem>();
        private readonly Dictionary<Guid, CachedRoute> _routes = new Dictionary<Guid, CachedRoute>();
        private readonly object _sync = new object();
        private readonly IPlanCacheExpirationStrategy _expirationStrategy;
        
        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

        public PlanCache(IPlanCacheExpirationStrategy expirationStrategy)
        {
            _expirationStrategy = expirationStrategy;
            expirationStrategy.SetExpirationCallback(RemoveExpiredRoutes);
        }
        
        /**********************************************************************************/

        public RouteNodeDO Get(Guid id, Func<Guid, RouteNodeDO> cacheMissCallback)
        {
            RouteNodeDO node;

            lock (_sync)
            {
                CacheItem cacheItem;

                if (!_routeNodesLookup.TryGetValue(id, out cacheItem))
                {
                    node = cacheMissCallback(id);

                    if (node == null)
                    {
                        return null;
                    }

                    // Get the root of RouteNode tree. 
                    while (node.ParentRouteNode != null)
                    {
                        node = node.ParentRouteNode;
                    }

                    // Check cache integrity
                    if (RouteTreeHelper.Linearize(node).Any(x => _routeNodesLookup.ContainsKey(x.Id)))
                    {
                        DropCachedRoute(node);
                    }

                    AddToCache(node);
                }
                else
                {
                    node = cacheItem.Route.Root;
                    // update route expiration
                    cacheItem.Route.Expiration = _expirationStrategy.NewExpirationToken();
                }

                node = RouteTreeHelper.CloneWithStructure(node);
            }

            return node;
        }

        /**********************************************************************************/
        
        public void UpdateElements(Action<RouteNodeDO> updater)
        {
            lock (_sync)
            {
                foreach (var cacheItem in _routeNodesLookup.Values)
                {
                    updater(cacheItem.Node);
                }
            }
        }

        /**********************************************************************************/

        public void UpdateElement(Guid id, Action<RouteNodeDO> updater)
        {
            lock (_sync)
            {
                CacheItem node;

                if (_routeNodesLookup.TryGetValue(id, out node))
                {
                    updater(node.Node);
                }
            }
        }

        /**********************************************************************************/

        public Changes Update(Guid planId, Changes changes)
        {
           var validChanges = new Changes();

            lock (_sync)
            {
                CachedRoute route;

                if (!_routes.TryGetValue(planId, out route))
                {
                    foreach (var insert in changes.Insert)
                    {
                        var clone = insert.Clone();

                        if (insert is PlanDO)
                        {
                            route = new CachedRoute((PlanDO) clone, _expirationStrategy.NewExpirationToken());
                            _routes.Add(planId, route);
                            _routeNodesLookup.Add(planId, new CacheItem(clone, route));
                            clone.RootRouteNode = clone;
                            break;
                        }
                    }
                }

                foreach (var insert in changes.Insert)
                {
                    if (!_routeNodesLookup.ContainsKey(insert.Id))
                    {
                        if (insert.ParentRouteNodeId == null || _routeNodesLookup.ContainsKey(insert.ParentRouteNodeId.Value))
                        {
                            _routeNodesLookup.Add(insert.Id, new CacheItem(insert.Clone(), route));
                        }
                    }
                }

                foreach (var insert in changes.Insert)
                {
                    CacheItem nodeCacheItem;

                    if (!_routeNodesLookup.TryGetValue(insert.Id, out nodeCacheItem))
                    {
                        continue;
                    }

                    validChanges.Insert.Add(insert);

                    var node = nodeCacheItem.Node;

                    if (insert.ParentRouteNodeId != null)
                    {
                        var parent = _routeNodesLookup[insert.ParentRouteNodeId.Value].Node;

                        parent.ChildNodes.Add(node);
                        node.ParentRouteNode = parent;
                        node.RootRouteNode = route.Root;
                    }
                }

                foreach (var deleted in changes.Delete)
                {
                    CachedRoute plan;

                    if (_routes.TryGetValue(deleted.Id, out plan))
                    {
                        RouteTreeHelper.Visit(plan.Root, x =>
                        {
                            _routeNodesLookup.Remove(x.Id);
                            validChanges.Delete.Add(x);
                        });

                        _routes.Remove(plan.Root.Id);

                        return validChanges;
                    }

                    CacheItem node;
                    if (_routeNodesLookup.TryGetValue(deleted.Id, out node))
                    {
                        validChanges.Delete.Add(deleted);
                        _routeNodesLookup.Remove(deleted.Id);
                        node.Node.RemoveFromParent();
                    }
                }

                foreach (var update in changes.Update)
                {
                    foreach (var changedProperty in update.ChangedProperties)
                    {
                        CacheItem originalCacheItem;

                        if (!_routeNodesLookup.TryGetValue(update.Node.Id, out originalCacheItem))
                        {
                            continue;
                        }

                        var original = originalCacheItem.Node;

                        // structure was changed
                        if (changedProperty.Name == "ParentRouteNodeId")
                        {
                            CacheItem parentCacheItem;

                            if (update.Node.ParentRouteNodeId == null || !_routeNodesLookup.TryGetValue(update.Node.ParentRouteNodeId.Value, out parentCacheItem))
                            {
                                continue;
                            }

                            var parent = parentCacheItem.Node;

                            original.RemoveFromParent();
                            parent.ChildNodes.Add(original);
                            original.ParentRouteNode = parent;

                            validChanges.Update.Add(update);
                        }
                        else
                        {
                            validChanges.Update.Add(update);
                            changedProperty.SetValue(original, changedProperty.GetValue(update.Node));
                        }
                    }
                }
            }

            return validChanges;
        }

        /**********************************************************************************/

        private void AddToCache(RouteNodeDO root)
        {
            var expirOn = _expirationStrategy.NewExpirationToken();
            var cachedRoute = new CachedRoute(root, expirOn);
            _routes.Add(root.Id, cachedRoute);

            RouteTreeHelper.Visit(root, x => _routeNodesLookup.Add(x.Id, new CacheItem(x, cachedRoute)));
        }

        /**********************************************************************************/

        private void DropCachedRoute(RouteNodeDO root)
        {
            CachedRoute cachedRoute;
            
            if (!_routes.TryGetValue(root.Id, out cachedRoute))
            {
                return;
            }

            RouteTreeHelper.Visit(root, x => _routeNodesLookup.Remove(x.Id));
            
            _routes.Remove(root.Id);
        }

        /**********************************************************************************/

        private void RemoveExpiredRoutes()
        {
            lock (_sync)
            {
                foreach (var routeExpiration in _routes.ToArray())
                {
                    if (routeExpiration.Value.Expiration.IsExpired())
                    {
                        _routes.Remove(routeExpiration.Key);
                        RouteTreeHelper.Visit(routeExpiration.Value.Root, x => _routeNodesLookup.Remove(x.Id));
                    }
                }
            }
        }

        /**********************************************************************************/
    }
}