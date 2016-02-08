using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories.Plan
{
    public class PlanRepository : IPlanRepository
    {
        /**********************************************************************************/
        // Declarations
        /**********************************************************************************/

        private readonly List<LoadedRoute>  _loadedRoutes = new List<LoadedRoute>();
        private readonly Dictionary<Guid, LoadedRoute> _loadedNodes = new Dictionary<Guid, LoadedRoute>();
        private readonly PlanStorage _planStorage;

        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

        public PlanRepository(PlanStorage planStorage)
        {
            _planStorage = planStorage;
        }

        /**********************************************************************************/

        private RouteNodeDO GetNewNodeOrGetExising(Dictionary<Guid, RouteNodeDO> exisingNodes, RouteNodeDO newNode)
        {
            if (newNode == null)
            {
                return null;
            }

            RouteNodeDO existingNode;

            if (exisingNodes.TryGetValue(newNode.Id, out existingNode))
            {
                return existingNode;
            }

            return newNode;
        }

        /**********************************************************************************/
        // This method updates locally cached elements from global cache or DB
        // This method will overwrite all local changes
        public TRouteNode Reload<TRouteNode>(Guid id)
              where TRouteNode : RouteNodeDO
        {
            var routeFromDb = GetRouteByMemberId(id);

            lock (_loadedNodes)
            {
                // have we already loaded this route?
                var loadedRoute = _loadedRoutes.FirstOrDefault(x => x.Root.Id == routeFromDb.Id);

                if (loadedRoute == null)
                {
                    //if no, then just get this route by id
                    return GetById<TRouteNode>(id);
                }

                routeFromDb = RouteTreeHelper.CloneWithStructure(routeFromDb);

                // get list of currently loaded items
                var currentNodes = RouteTreeHelper.Linearize(loadedRoute.Root).ToDictionary(x => x.Id, x => x);
                var dbNodes = RouteTreeHelper.Linearize(routeFromDb).ToDictionary(x => x.Id, x => x);

                foreach (var routeNodeDo in dbNodes)
                {
                    RouteNodeDO currentNode;

                    // sync structure
                    var originalChildren = routeNodeDo.Value.ChildNodes;
                    routeNodeDo.Value.ChildNodes = new List<RouteNodeDO>(originalChildren.Count);

                    foreach (var childNode in originalChildren)
                    {
                        routeNodeDo.Value.ChildNodes.Add(GetNewNodeOrGetExising(currentNodes, childNode));
                    }

                    routeNodeDo.Value.ParentRouteNode = GetNewNodeOrGetExising(currentNodes, routeNodeDo.Value.ParentRouteNode);
                    routeNodeDo.Value.RootRouteNode = GetNewNodeOrGetExising(currentNodes, routeNodeDo.Value.RootRouteNode);

                    if (currentNodes.TryGetValue(routeNodeDo.Key, out currentNode))
                    {
                        //sync local cached properties with db one
                        currentNode.SyncPropertiesWith(routeNodeDo.Value);
                        currentNode.ChildNodes = routeNodeDo.Value.ChildNodes;
                        currentNode.ParentRouteNode = routeNodeDo.Value.ParentRouteNode;
                        currentNode.RootRouteNode = routeNodeDo.Value.RootRouteNode;
                    }
                    else // we don't have this node in our local cache.
                    {
                        _loadedNodes[routeNodeDo.Key] = loadedRoute;
                    }
                }

                // remove nodes, that we deleted in the DB version
                foreach (var routeNodeDo in currentNodes)
                {
                    if (!dbNodes.ContainsKey(routeNodeDo.Key))
                    {
                        _loadedNodes.Remove(routeNodeDo.Key);
                    }
                }

                return (TRouteNode)loadedRoute.Find(id);
            }
        }

        /**********************************************************************************/

        public TRouteNode Reload<TRouteNode>(Guid? id)
            where TRouteNode : RouteNodeDO
        {
            if (id == null)
            {
                return null;
            }

            return Reload<TRouteNode>(id.Value);
        }

        /**********************************************************************************/

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

                    route = RouteTreeHelper.CloneWithStructure(route);

                    loadedRoute = new LoadedRoute(route);
                    _loadedRoutes.Add(loadedRoute);
                    // add all noded to the loaded nodes list
                    RouteTreeHelper.Visit(route, x => _loadedNodes.Add(x.Id, loadedRoute));
                }

                // search for the node in the corresponding routes
                return (TRouteNode)loadedRoute.Find(id);
            }
        }

        /**********************************************************************************/

        public TRouteNode GetById<TRouteNode>(Guid? id)
          where TRouteNode : RouteNodeDO
        {
            if (id == null)
            {
                return null;
            }

            return GetById<TRouteNode>(id.Value);
        }
        
        /**********************************************************************************/
        // this is just simplification for the first implementation.
        // We can only insert plans. If we want to edit plan, we need to get corresponding node and edit it's children
        public void Add(PlanDO plan)
        {
            lock (_loadedNodes)
            {
                var loadedRoute = new LoadedRoute(plan, true);
               
                RouteTreeHelper.Visit(plan, x =>
                {
                    if (x.Id == Guid.Empty)
                    {
                        x.Id = Guid.NewGuid();
                    }
                    
                    _loadedNodes.Add(x.Id, loadedRoute);
                });
                _loadedRoutes.Add(loadedRoute);
            }
        }

        /**********************************************************************************/
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
                    if (loadedRoute.IsNew)
                    {
                        _loadedNodes.Remove(node.Id);
                        _loadedRoutes.Remove(loadedRoute);
                    }
                    else
                    {
                        loadedRoute.IsDeleted = true;
                    }
                }
            }
        }

        /**********************************************************************************/
        // Methods that don't work with our cache. They always make DB requests.
        // !!!NEVER EVER EDIT!!! objects returned by these methods
        // Or you get the cache out of sync
        public IQueryable<PlanDO> GetPlanQueryUncached()
        {
            return _planStorage.GetPlanQuery();
        }

        /**********************************************************************************/

        public IQueryable<ActivityDO> GetActivityQueryUncached()
        {
            return _planStorage.GetActivityQuery();
        }

        /**********************************************************************************/

        public IQueryable<RouteNodeDO> GetNodesQueryUncached()
        {
            return _planStorage.GetNodesQuery();
        }

        // *************************************************************
        // Workaround to maintain cache integrity in authorization token revoke scenario
        public void RemoveAuthorizationTokenFromCache(ActivityDO activity)
        {
            lock (_loadedNodes)
            {
                foreach (var loadedRoute in _loadedRoutes)
                {
                     RouteTreeHelper.Visit(loadedRoute.Root, x =>
                     {
                         var a = x as ActivityDO;

                         if (a != null && a.Id == activity.Id)
                         {
                             a.AuthorizationToken = null;
                             a.AuthorizationTokenId = null;
                         }
                     });
                }
            }

            _planStorage.UpdateElement(activity.Id, x =>
            {
                var a = x as ActivityDO;
                
                if (a != null)
                {
                    a.AuthorizationToken = null;
                    a.AuthorizationTokenId = null;
                }
            });
        }
        
        /**********************************************************************************/

        public void SaveChanges()
        {
            lock (_loadedNodes)
            {
                foreach (var loadedRoute in _loadedRoutes)
                {
                    var route = loadedRoute;
                    var parentPlan = loadedRoute.Root as PlanDO;

                    RouteTreeHelper.Visit(loadedRoute.Root, (x, y) =>
                    {
                        if (x.Id == Guid.Empty)
                        {
                            x.Id = Guid.NewGuid();
                        }

                        x.ParentRouteNode = y;
                        x.ParentRouteNodeId = y != null ? y.Id : (Guid?) null;

                        if (parentPlan != null)
                        {
                            x.Fr8AccountId = parentPlan.Fr8AccountId;
                            x.Fr8Account = parentPlan.Fr8Account;
                            x.RootRouteNodeId = parentPlan.Id;

                            UpdateForeignKeys(x);
                        }
                        else
                        {
                            UpdateForeignKeys(x);
                        }

                        _loadedNodes[x.Id] = route;
                    });

                    var clonedRoute = RouteTreeHelper.CloneWithStructure(loadedRoute.Root);
                    _planStorage.Update(clonedRoute);
                }
            }
        }

        /**********************************************************************************/
        // update Ids of foreign keys.
        private void UpdateForeignKeys(RouteNodeDO node)
        {
            if (node.Fr8Account != null)
            {
                node.Fr8AccountId = node.Fr8Account.Id;
            }

            if (node is ActivityDO)
            {
                UpdateForeignKeys((ActivityDO)node);
            }
        }

        /**********************************************************************************/

        private void UpdateForeignKeys(ActivityDO activity)
        {
            if (activity.AuthorizationToken != null)
            {
                activity.AuthorizationTokenId = activity.AuthorizationToken.Id;
            }
        }

        /**********************************************************************************/

        private RouteNodeDO GetRouteByMemberId(Guid id)
        {
            return _planStorage.LoadPlan(id);
        }
        
        /**********************************************************************************/

    }
}
