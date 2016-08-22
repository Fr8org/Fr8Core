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

        private readonly List<LoadedPlan>  _loadedPlans = new List<LoadedPlan>();
        private readonly Dictionary<Guid, LoadedPlan> _loadedNodes = new Dictionary<Guid, LoadedPlan>();
        private readonly PlanStorage _planStorage;

        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

        public PlanRepository(PlanStorage planStorage)
        {
            _planStorage = planStorage;
        }

        /**********************************************************************************/

        private PlanNodeDO GetNewNodeOrGetExising(Dictionary<Guid, PlanNodeDO> exisingNodes, PlanNodeDO newNode)
        {
            if (newNode == null)
            {
                return null;
            }

            PlanNodeDO existingNode;

            if (exisingNodes.TryGetValue(newNode.Id, out existingNode))
            {
                return existingNode;
            }

            return newNode;
        }

        /**********************************************************************************/
        // This method updates locally cached elements from global cache or DB
        // This method will overwrite all local changes
        public TPlanNode Reload<TPlanNode>(Guid id)
              where TPlanNode : PlanNodeDO
        {
            var planFromDb = GetPlanByMemberId(id);

            lock (_loadedNodes)
            {
                // have we already loaded this plan?
                var loadedPlan = _loadedPlans.FirstOrDefault(x => x.Root.Id == planFromDb.Id);

                if (loadedPlan == null)
                {
                    //if no, then just get this plan by id
                    return GetById<TPlanNode>(id);
                }
                
                // get list of currently loaded items
                var currentNodes = PlanTreeHelper.Linearize(loadedPlan.Root).ToDictionary(x => x.Id, x => x);
                var dbNodes = PlanTreeHelper.Linearize(planFromDb).ToDictionary(x => x.Id, x => x);

                foreach (var planNodeDo in dbNodes)
                {
                    PlanNodeDO currentNode;

                    // sync structure
                    var originalChildren = planNodeDo.Value.ChildNodes;
                    planNodeDo.Value.ChildNodes = new List<PlanNodeDO>(originalChildren.Count);

                    foreach (var childNode in originalChildren)
                    {
                        planNodeDo.Value.ChildNodes.Add(GetNewNodeOrGetExising(currentNodes, childNode));
                    }

                    planNodeDo.Value.ParentPlanNode = GetNewNodeOrGetExising(currentNodes, planNodeDo.Value.ParentPlanNode);
                    planNodeDo.Value.RootPlanNode = GetNewNodeOrGetExising(currentNodes, planNodeDo.Value.RootPlanNode);

                    if (currentNodes.TryGetValue(planNodeDo.Key, out currentNode))
                    {
                        //sync local cached properties with db one
                        currentNode.SyncPropertiesWith(planNodeDo.Value);
                        currentNode.ChildNodes = planNodeDo.Value.ChildNodes;
                        currentNode.ParentPlanNode = planNodeDo.Value.ParentPlanNode;
                        currentNode.RootPlanNode = planNodeDo.Value.RootPlanNode;
                    }
                    else // we don't have this node in our local cache.
                    {
                        _loadedNodes[planNodeDo.Key] = loadedPlan;
                    }
                }

                // remove nodes, that we deleted in the DB version
                foreach (var planNodeDo in currentNodes)
                {
                    if (!dbNodes.ContainsKey(planNodeDo.Key))
                    {
                        _loadedNodes.Remove(planNodeDo.Key);
                    }
                }

                loadedPlan.RebuildSnapshot();
                return (TPlanNode)loadedPlan.Find(id);
            }
        }

        /**********************************************************************************/

        public TPlanNode Reload<TPlanNode>(Guid? id)
            where TPlanNode : PlanNodeDO
        {
            if (id == null)
            {
                return null;
            }

            return Reload<TPlanNode>(id.Value);
        }

        /**********************************************************************************/

        public TPlanNode GetById<TPlanNode>(Guid id)
            where TPlanNode : PlanNodeDO
        {
            lock (_loadedNodes)
            {
                LoadedPlan loadedPlan;
                
                // if we have loaded this node before?
                if (!_loadedNodes.TryGetValue(id, out loadedPlan))
                {
                    // try to load plan for this node
                    var plan = GetPlanByMemberId(id);

                    // non existent node or new node that has not been saved yet
                    if (plan == null)
                    {
                        return null;
                    }
                    
                    loadedPlan = new LoadedPlan(plan);
                    _loadedPlans.Add(loadedPlan);
                    // add all noded to the loaded nodes list
                    PlanTreeHelper.Visit(plan, x => _loadedNodes.Add(x.Id, loadedPlan));
                }

                // search for the node in the corresponding plans
                return (TPlanNode)loadedPlan.Find(id);
            }
        }

        /**********************************************************************************/

        public TPlanNode GetById<TPlanNode>(Guid? id)
          where TPlanNode : PlanNodeDO
        {
            if (id == null)
            {
                return null;
            }

            return GetById<TPlanNode>(id.Value);
        }
        
        /**********************************************************************************/
        // this is just simplification for the first implementation.
        // We can only insert plans. If we want to edit plan, we need to get corresponding node and edit it's children
        public void Add(PlanDO plan)
        {
            lock (_loadedNodes)
            {
                var loadedPlan = new LoadedPlan(plan, true);
               
                PlanTreeHelper.Visit(plan, x =>
                {
                    if (x.Id == Guid.Empty)
                    {
                        x.Id = Guid.NewGuid();
                    }
                    
                    _loadedNodes.Add(x.Id, loadedPlan);
                });
                _loadedPlans.Add(loadedPlan);
            }
        }

        /**********************************************************************************/
        // this is just simplification the first implementation.
        // We can only delete plans. If we want to edit plan, we need to get corresponding node and edit it's children
        public void Delete(PlanDO node)
        {
            var plan = GetById<PlanNodeDO>(node.Id);
            if (plan == null)
            {
                return;
            }

            lock (_loadedNodes)
            {
                LoadedPlan loadedPlan;

                // if we have loaded this node before?
                if (_loadedNodes.TryGetValue(node.Id, out loadedPlan))
                {
                    if (loadedPlan.IsNew)
                    {
                        _loadedNodes.Remove(node.Id);
                        _loadedPlans.Remove(loadedPlan);
                    }
                    else
                    {
                        loadedPlan.IsDeleted = true;
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

        public IQueryable<PlanNodeDO> GetNodesQueryUncached()
        {
            return _planStorage.GetNodesQuery();
        }

        // *************************************************************
        // Workaround to maintain cache integrity in authorization token revoke scenario
        public void RemoveAuthorizationTokenFromCache(ActivityDO activity)
        {
            lock (_loadedNodes)
            {
                foreach (var loadedPlan in _loadedPlans)
                {
                     PlanTreeHelper.Visit(loadedPlan.Root, x =>
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
                foreach (var loadedPlan in _loadedPlans)
                {
                    var plan = loadedPlan;
                    plan.Root.LastUpdated = DateTimeOffset.UtcNow;
                    var parentPlan = loadedPlan.Root as PlanDO;

                    PlanTreeHelper.Visit(loadedPlan.Root, (x, y) =>
                    {
                        if (x.Id == Guid.Empty)
                        {
                            x.Id = Guid.NewGuid();
                        }

                        x.ParentPlanNode = y;
                        x.ParentPlanNodeId = y != null ? y.Id : (Guid?) null;

                        if (parentPlan != null)
                        {
                            x.Fr8AccountId = parentPlan.Fr8AccountId;
                            x.Fr8Account = parentPlan.Fr8Account;
                            x.RootPlanNodeId = parentPlan.Id;

                            UpdateForeignKeys(x);
                        }
                        else
                        {
                            UpdateForeignKeys(x);
                        }

                        _loadedNodes[x.Id] = plan;
                    });

                    var previous = loadedPlan.RebuildSnapshot();
                    var changes = loadedPlan.Snapshot.Compare(previous);

                    _planStorage.Update(loadedPlan.Root.Id, changes);
                }
            }
        }

        /**********************************************************************************/
        // update Ids of foreign keys.
        private void UpdateForeignKeys(PlanNodeDO node)
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

        private PlanNodeDO GetPlanByMemberId(Guid id)
        {
            return _planStorage.LoadPlan(id);
        }
        
        /**********************************************************************************/

    }
}
