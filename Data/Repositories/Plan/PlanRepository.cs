using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Resources;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Data.Migrations;
using Data.States;
using StructureMap.Pipeline.Lazy;
using Utilities;

namespace Data.Repositories.Plan
{
    internal class RouteNodeProxyDO : RouteNodeDO
    {
        public override RouteNodeDO ParentRouteNode
        {
            get { return base.ParentRouteNode; }
            set { base.ParentRouteNode = value; }
        }

        public override IList<RouteNodeDO> ChildNodes
        {
            get { return base.ChildNodes; }
            set { base.ChildNodes = value; }
        }  
    }




    internal class ActivityProxyDO : ActivityDO
    {
        public ActivityProxyDO()
        {
        }

      
    }


    internal class StructureIndex
    {
        private readonly Dictionary<Guid, List<RouteNodeDO>> _childrenIndex = new Dictionary<Guid, List<RouteNodeDO>>();

        public void AddChild(Guid parent, RouteNodeDO child)
        {
            List<RouteNodeDO> children;

            if (!_childrenIndex.TryGetValue(parent, out children))
            {
                children = new List<RouteNodeDO>();
                _childrenIndex[parent] = children;
            }

            children.Add(child);
        }

        public void RemoveChild(Guid parent, RouteNodeDO child)
        {
            List<RouteNodeDO> children;

            if (_childrenIndex.TryGetValue(parent, out children))
            {
                children.RemoveAll(x => x.Id == child.Id);
            }
        }

        public void Remove(RouteNodeDO node)
        {
            _childrenIndex.Remove(node.Id);

            if (node.ParentRouteNodeId != null)
            {
                _childrenIndex[node.ParentRouteNodeId.Value].RemoveAll(x => x.Id == node.Id);
            }
        }
    }

    /*
    internal class NodesStructure
    {
        private class NodeRef
        {
            public bool IsDeleted;
            public Guid Guid;
            public RouteNodeDO Node;
        }

        private Dictionary<Guid, List<NodeRef>> _structure = new Dictionary<Guid, List<NodeRef>>();

        private Dictionary<Guid, NodeRef>  _refs = new Dictionary<Guid, NodeRef>();

        private NodeRef ResolveRef(RouteNodeDO node)
        {
            NodeRef reference;

            if (_refs.TryGetValue(node.Id, out reference))
            {
                return reference;
            }

            reference = new NodeRef()
            {
                Guid = node.Id,
                Node = node
            };

            _refs[reference.Guid] = reference;
            
            return reference;
        }

        public void AddChild(Guid parent, RouteNodeDO child)
        {
            List<NodeRef> children;

            if (!_structure.TryGetValue(parent, out children))
            {
                children = new List<NodeRef>();
                _structure[parent] = children;
            }

            children.Add(ResolveRef(child));
        }

        public void RemoveChild(Guid parent, RouteNodeDO child)
        {
            List<NodeRef> children;

            if (_structure.TryGetValue(parent, out children))
            {
                for (int i = children.Count - 1; i >= 0; i --)
                {
                    if (children[i].Guid == child.Id)
                    {
                        children.RemoveAt(i);
                    }
                }
            }
        }

        public 

        public void Remove(Guid node)
        {
            NodeRef reference;

            if (_refs.TryGetValue(node, out reference))
            {
                reference.IsDeleted = true;
                _refs.Remove(node);
            }

            _structure.Remove(node);
        }

    }*/


    public class PlanRepository : Interfaces.IPlanRepository
    {
        private readonly IUnitOfWork _uow;
        private readonly MemoryCache _routeCache;
        private readonly Dictionary<Guid, LoadedRoute> _loadedNodes = new Dictionary<Guid, LoadedRoute>();
        private readonly TimeSpan _expiration = TimeSpan.FromMinutes(1);
        private readonly PlanCache _cache;
            
        private readonly RouteNodeRepository _routeNodes;
        private readonly ActivityRepository _activityRepository;
        private readonly SubrouteRepository _subroutes;
        private readonly RouteRepository _routes;
        
        public PlanRepository(IUnitOfWork uow)
        {
            _uow = uow;
            _routeNodes = new RouteNodeRepository(uow);
            _activityRepository = new ActivityRepository(uow);
            _subroutes = new SubrouteRepository(uow);
            _routes = new RouteRepository(uow);
        }
        
        public TRouteNode GetById<TRouteNode>(Guid id)
            where TRouteNode : RouteNodeDO
        {
            lock (_loadedNodes)
            {
                LoadedRoute loadedRoute;
                
                // if we have loaded this node before?
                if (!_loadedNodes.TryGetValue(id, out loadedRoute))
                {
                    // try to load route for this node
                    var route = GetRouteByMemberId(id);

                    // non existent node or new node that has not been saved yet
                    if (route == null)
                    {
                        return null;
                    }

                    loadedRoute = new LoadedRoute(route);
                    // add all noded to the loaded nodes list
                    RouteTreeHelper.Visit(route, x => _loadedNodes.Add(x.Id, loadedRoute));
                }

                // search for the node in the corresponding routes
                return (TRouteNode)loadedRoute.Find(id);
            }
        }

        public TRouteNode GetById<TRouteNode>(Guid? id)
          where TRouteNode : RouteNodeDO
        {
            if (id == null)
            {
                return null;
            }

            return GetById<TRouteNode>(id.Value);
        }

        // this is just simplification for the first implementation.
        // We can only insert plans. If we want to edit plan, we need to get corresponding node and edit it's children
        public void Add(PlanDO plan)
        {
            lock (_loadedNodes)
            {
                var loadedRoute = new LoadedRoute(plan);
                RouteTreeHelper.Visit(plan, x => _loadedNodes.Add(x.Id, loadedRoute));
            }
        }

        // this is just simplification the first implementation.
        // We can only delete plans. If we want to edit plan, we need to get corresponding node and edit it's children
        public void Delete(PlanDO node)
        {
            var route = GetById<RouteNodeDO>(node.Id);
            if (route == null)
            {
                return;
            }

            lock (_loadedNodes)
            {
                LoadedRoute loadedRoute;

                // if we have loaded this node before?
                if (_loadedNodes.TryGetValue(node.Id, out loadedRoute))
                {
                    loadedRoute.IsDeleted = true;
                }
            }
        }

        // *************************************************************
        // Methods that don't work with our cache. They always make DB requests.
        // !!!NEVER EVER EDIT!!! objects returned by these methods
        // Or you get the cache out of sync
        public IQueryable<PlanDO> GetPlanQueryUncached()
        {
            return _routes.GetQuery();
        }

        public IQueryable<ActivityDO> GetActivityQueryUncached()
        {
            return _activityRepository.GetQuery();
        }

        public IQueryable<RouteNodeDO> GetNodesQueryUncached()
        {
            return _routeNodes.GetQuery();
        }

        // *************************************************************
        // Workaround to maintain cache integrity in authorization token revoke scenario
        public void RemoveAuthorizationTokenFromCache(ActivityDO activity)
        {
            throw new NotImplementedException();
        }

        // *************************************************************

        private RouteNodeDO GetRouteByMemberId(Guid id)
        {
            return null;
        }

        private RouteNodeDO LoadRoute(Guid seedId)
        {
            lock (_routeCache)
            {
                var value = _routeCache.Get(seedId.ToString("N"));
                
                if (value != null)
                {
                    return (RouteNodeDO)value;
                }

                var seed = _routeNodes.GetQuery().FirstOrDefault(x => x.Id == seedId);


                var routes = _routes.GetQuery().Where(x => x.Id == seed.RootRouteNodeId).ToArray();
                var actions = _activityRepository.GetQuery().Where(x => x.RootRouteNodeId == seed.RootRouteNodeId).Include(x => x.AuthorizationToken).ToArray();
                var subroutes = _subroutes.GetQuery().Where(x => x.RootRouteNodeId == seed.RootRouteNodeId).ToArray();




                // var floatRoute = _uow.RouteNodeRepository.GetQuery().Where(x => x.RootRouteNodeId == seed.ParentRouteNodeId);


            }

            return null;
        }

    }
}
