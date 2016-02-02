using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories.Plan
{
    public class PlanRepository : IPlanRepository
    {
        private readonly PlanStorage _planStorage;
        /**********************************************************************************/
        // Declarations
        /**********************************************************************************/

        private readonly List<LoadedRoute>  _loadedRoutes = new List<LoadedRoute>();
        private readonly Dictionary<Guid, LoadedRoute> _loadedNodes = new Dictionary<Guid, LoadedRoute>();
        private readonly IPlanCache _cache;
        private readonly IPlanStorageProvider _storageProvider;

        /**********************************************************************************/
        // Functions
        /**********************************************************************************/
        
        public PlanRepository(PlanStorage planStorage)
        {
            _planStorage = planStorage;
            //_cache = cache; 
           // _storageProvider = storageProvider;
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
                    RouteTreeHelper.Visit(loadedRoute.Root, (x, y) =>
                    {
                        if (x.Id == Guid.Empty)
                        {
                            x.Id = Guid.NewGuid();
                        }

                        x.ParentRouteNode = y;
                        x.ParentRouteNodeId = y != null ? y.Id : (Guid?) null;
                        _loadedNodes[x.Id] = route;
                    });

                    var parentPlan = loadedRoute.Root as PlanDO;
                    Action<RouteNodeDO> updateCallback = null;

                    if (parentPlan != null)
                    {
                        updateCallback = x =>
                        {
                            x.Fr8AccountId = parentPlan.Fr8AccountId;
                            x.Fr8Account = parentPlan.Fr8Account;
                            x.RootRouteNodeId = parentPlan.Id;
                        };
                    }

                    var clonedRoute = RouteTreeHelper.CloneWithStructure(loadedRoute.Root, updateCallback);
                    _planStorage.Update(clonedRoute);
//                    var currentSnapshot = new RouteSnapshot(clonedRoute, true);
//                    
//                    var changes = currentSnapshot.Compare(loadedRoute.Snapshot);
//
//                    if (changes.HasChanges)
//                    {
//                        _storageProvider.Update(changes);
//                        _cache.Update(clonedRoute);
//                        loadedRoute.Snapshot = currentSnapshot;
//                    }
                }
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
