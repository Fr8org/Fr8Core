using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;

namespace Data.Repositories.Plan
{
    public interface IExpirationToken
    {
        bool IsExpired();
    }

    public interface IPlanCacheExpirationStrategy
    {
        void SetExpirationCallback(Action callback);
        IExpirationToken NewExpirationToken();
    }
    
    public class PlanCache
    {
        private class CachedRoute
        {
            public RouteNodeDO Root { get; private set; }

            public IExpirationToken Expiration { get; set; }

            public CachedRoute(RouteNodeDO root, IExpirationToken expiration)
            {
                Root = root;
                Expiration = expiration;
            }

            public List<RouteNodeDO> Linearize()
            {
                return PlanCache.Linearize(Root);
            }
        }

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

        private readonly Dictionary<Guid, CacheItem> _routeNodesLookup = new Dictionary<Guid, CacheItem>();
        private readonly Dictionary<Guid, CachedRoute> _routes = new Dictionary<Guid, CachedRoute>();
        private readonly object _sync = new object();
        private readonly IPlanCacheExpirationStrategy _expirationStrategy;
        private readonly Func<Guid, RouteNodeDO> _cacheMissCallback;

        public PlanCache(IPlanCacheExpirationStrategy expirationStrategy, Func<Guid, RouteNodeDO> cacheMissCallback)
        {
            _expirationStrategy = expirationStrategy;
            _cacheMissCallback = cacheMissCallback;
            expirationStrategy.SetExpirationCallback(RemoveExpiredRoutes);
        }

        private static List<RouteNodeDO> Linearize(RouteNodeDO root)
        {
            var nodes = new List<RouteNodeDO>();
            Linearize(root, nodes);

            return nodes;
        }

        private static void Linearize(RouteNodeDO root, List<RouteNodeDO> nodes)
        {
            nodes.Add(root);

            foreach (var routeNodeDo in root.ChildNodes)
            {
                Linearize(routeNodeDo, nodes);
            }
        }

        public RouteNodeDO Get(Guid id)
        {
            RouteNodeDO node;

            lock (_sync)
            {
                CacheItem cacheItem;

                if (!_routeNodesLookup.TryGetValue(id, out cacheItem))
                {
                    node =  _cacheMissCallback(id);

                    // Get the root of RouteNode tree. 
                    while (node.ParentRouteNode != null)
                    {
                        node = node.ParentRouteNode;
                    }

                    // Check cache integrity
                    if (Linearize(node).Any(x => _routeNodesLookup.ContainsKey(x.Id)))
                    {
                        DropCachedRoute(node);
                    }

                    AddToCache(node);
                }
                else
                {
                    node = cacheItem.Route.Root;
                    // update route sliding expiration
                    cacheItem.Route.Expiration = _expirationStrategy.NewExpirationToken();
                }
            }

            return node;
        }

        public void Update(RouteNodeDO node)
        {
            // Get the root of RouteNode tree. 
            while (node.ParentRouteNode != null)
            {
                node = node.ParentRouteNode;
            }

            DropCachedRoute(node);
            AddToCache(node);
        }

        private void AddToCache(RouteNodeDO root)
        {
            var expirOn = _expirationStrategy.NewExpirationToken();
            var cachedRoute = new CachedRoute(root, expirOn);
            _routes.Add(root.Id, cachedRoute);
            
            foreach (var routeNodeDo in Linearize(root))
            {
                _routeNodesLookup.Add(routeNodeDo.Id, new CacheItem(routeNodeDo, cachedRoute));
            }
        }

        private void DropCachedRoute(RouteNodeDO root)
        {
            CachedRoute cachedRoute;
            
            if (!_routes.TryGetValue(root.Id, out cachedRoute))
            {
                return;
            }

            foreach (var node in cachedRoute.Linearize())
            {
                _routeNodesLookup.Remove(node.Id);
            }

            _routes.Remove(root.Id);
        }

        private void RemoveExpiredRoutes()
        {
            lock (_sync)
            {
                foreach (var routeExpiration in _routes.ToArray())
                {
                    if (routeExpiration.Value.Expiration.IsExpired())
                    {
                        foreach (var node in routeExpiration.Value.Linearize())
                        {
                            _routeNodesLookup.Remove(node.Id);
                        }
                        
                        _routes.Remove(routeExpiration.Key);
                    }
                }
            }
        }
    }
}