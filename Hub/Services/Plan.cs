using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using AutoMapper;
using Hub.Exceptions;
using Microsoft.AspNet.Identity.EntityFramework;
using StructureMap;
using Data.Entities;
using Data.Exceptions;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Interfaces;
using InternalInterface = Hub.Interfaces;
using Hub.Managers;
using System.Threading.Tasks;
using Data.Crates;
using Data.Infrastructure;

namespace Hub.Services
{
    public class Plan : IPlan
    {
        // private readonly IProcess _process;
        private readonly InternalInterface.IContainer _container;
        private readonly ISubroute _subroute;
        private readonly Fr8Account _dockyardAccount;
        private readonly IAction _action;
        private readonly IRouteNode _activity;
        private readonly ICrateManager _crate;
        private readonly ISecurityServices _security;
        private readonly IProcessNode _processNode;

        public Plan()
        {
            _container = ObjectFactory.GetInstance<InternalInterface.IContainer>();
            _subroute = ObjectFactory.GetInstance<ISubroute>();
            _dockyardAccount = ObjectFactory.GetInstance<Fr8Account>();
            _action = ObjectFactory.GetInstance<IAction>();
            _activity = ObjectFactory.GetInstance<IRouteNode>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
            _security = ObjectFactory.GetInstance<ISecurityServices>();
            _processNode = ObjectFactory.GetInstance<IProcessNode>();
        }

        public IList<PlanDO> GetForUser(IUnitOfWork unitOfWork, Fr8AccountDO account, bool isAdmin = false, Guid? id = null, int? status = null)
        {
            var queryableRepo = unitOfWork.RouteRepository.GetQuery().Include(pt => pt.ChildContainers); // whe have to include Activities as it is a real navigational property. Not Routes

            if (isAdmin)
            {
                queryableRepo = (id == null ? queryableRepo : queryableRepo.Where(pt => pt.Id == id));
                return (status == null ? queryableRepo : queryableRepo.Where(pt => pt.RouteState == status)).ToList();
            }

            queryableRepo = (id == null
                ? queryableRepo.Where(pt => pt.Fr8Account.Id == account.Id)
                : queryableRepo.Where(pt => pt.Id == id && pt.Fr8Account.Id == account.Id));
            return (status == null
                ? queryableRepo : queryableRepo.Where(pt => pt.RouteState == status)).ToList();

        }

        public IList<PlanDO> GetByName(IUnitOfWork uow, Fr8AccountDO account, string name)
        {
            return uow.RouteRepository.GetQuery().Where(r => r.Fr8Account.Id == account.Id && r.Name == name).ToList();
        }

        public void CreateOrUpdate(IUnitOfWork uow, PlanDO ptdo, bool updateChildEntities)
        {
            var creating = ptdo.Id == Guid.Empty;
            if (creating)
            {
                ptdo.Id = Guid.NewGuid();
                ptdo.RouteState = RouteState.Inactive;

                var subroute = new SubrouteDO(true);
                subroute.Id = Guid.NewGuid();
                subroute.RootRouteNode = ptdo;
                subroute.ParentRouteNode = ptdo;
                subroute.Fr8Account = ptdo.Fr8Account;
                ptdo.ChildNodes.Add(subroute);

                uow.RouteRepository.Add(ptdo);
                _subroute.Store(uow, ptdo.StartingSubroute);
            }
            else
            {
                var curPlan = uow.RouteRepository.GetByKey(ptdo.Id);
                if (curPlan == null)
                    throw new EntityNotFoundException();
                curPlan.Name = ptdo.Name;
                curPlan.Description = ptdo.Description;
                // ChildEntities update code has been deleted by demel 09/28/2015
            }
            //uow.SaveChanges(); we don't want to save changes here. we want the calling method to get to decide when this uow should be saved as a group
            // return ptdo.Id;
        }

        public PlanDO Create(IUnitOfWork uow, string name)
        {
            var plan = new PlanDO()
            {
                Id = Guid.NewGuid(),
                Name = name
            };

            if (plan.Fr8Account == null)
            {
                plan.Fr8Account = _security.GetCurrentAccount(uow);
            }

            plan.RouteState = RouteState.Inactive;
            uow.RouteRepository.Add(plan);

            return plan;
        }

        public void Delete(IUnitOfWork uow, Guid id)
        {
            var curPlan = uow.RouteRepository.GetQuery().Include(c => c.ChildContainers).SingleOrDefault(pt => pt.Id == id);

            if (curPlan == null)
            {
                throw new EntityNotFoundException<PlanDO>(id);
            }

            curPlan.RouteState = RouteState.Deleted;

            //Route deletion will only update its RouteState = Deleted
            //_activity.Delete(uow, curPlan);

            var containers = curPlan.ChildContainers;
            if (containers != null)
            {
                foreach (var container in containers)
                {
                    container.ContainerState = ContainerState.Deleted;
                }
            }
        }

        public IList<SubrouteDO> GetSubroutes(PlanDO curPlanDO)
        {
            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //var queryableRepo = unitOfWork.SubrouteRepository
                //.GetQuery()
                //.Include(x => x.ChildNodes)
                //.Where(x => x.ParentRouteNodeId == curPlanDO.Id)
                //.OrderBy(x => x.Id)
                //.ToList();

                //return queryableRepo;

                var queryableRepo = unitOfWork.RouteRepository.GetQuery()
                    .Include("Subroutes")
                    .Where(x => x.Id == curPlanDO.Id);

                return queryableRepo.SelectMany<PlanDO, SubrouteDO>(x => x.Subroutes)
                    .ToList();
            }
        }



        private IEnumerable<TActivity> EnumerateActivities<TActivity>(PlanDO curPlan, bool allowOnlyOneNoteTemplate = true)
        {
            bool firstNodeTemplate = true;

            foreach (SubrouteDO template in curPlan.Subroutes)
            {
                if (allowOnlyOneNoteTemplate && !firstNodeTemplate)
                {
                    throw new Exception("More than one process node template with non empty list of action exsists in the process");
                }

                firstNodeTemplate = false;

                if (template.ChildNodes != null)
                {
                    foreach (var activityDo in template.ChildNodes.OfType<TActivity>())
                    {
                        yield return activityDo;
                    }
                }
            }
        }

        /// <summary>
        /// Iterates all RouteNode tree by traversing through children
        /// </summary>
        /// <typeparam name="TActivity"></typeparam>
        /// <param name="rootNode"></param>
        /// <returns></returns>
        private IEnumerable<TActivity> EnumerateActivityTree<TActivity>(RouteNodeDO rootNode) where TActivity : RouteNodeDO
        {
            var routeNodeQueue = new Queue<RouteNodeDO>();
            routeNodeQueue.Enqueue(rootNode);

            while (routeNodeQueue.Count > 0)
            {
                var result = routeNodeQueue.Dequeue();
                if (result is TActivity)
                {
                    yield return result as TActivity;
                }

                foreach (var activityDo in result.ChildNodes.OfType<TActivity>())
                    routeNodeQueue.Enqueue(activityDo);
            }
        }



        public async Task<ActivateActionsDTO> Activate(Guid curPlanId, bool routeBuilderActivate)
        {
            var result = new ActivateActionsDTO
            {
                Status = "no action",
                ActionsCollections = new List<ActionDTO>()
            };

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = uow.RouteRepository.GetByKey(curPlanId);

                if (plan.Subroutes == null)
                {
                    throw new ArgumentNullException("Parameter Subroutes is null.");
                }

                foreach (SubrouteDO template in plan.Subroutes)
                {
                    var activities = EnumerateActivityTree<ActionDO>(template);
                    foreach (var curActionDO in activities)
                    {
                        try
                        {
                            var resultActivate = await _action.Activate(curActionDO);

                            string errorMessage;
                            result.Status = "success";

                            var validationErrorChecker = CheckForExistingValidationErrors(resultActivate, out errorMessage);
                            if (validationErrorChecker)
                            {
                                result.Status = "validation_error";
                                result.ErrorMessage = errorMessage;
                            }

                            //if the activate call is comming from the Route Builder just render again the action group with the errors
                            if (routeBuilderActivate)
                            {
                                result.ActionsCollections.Add(resultActivate);
                            }
                            else if(validationErrorChecker)
                            {
                                //if the activate call is comming from the Routes List then show the first error message and redirect to plan builder 
                                //so the user could fix the configuration
                                result.RedirectToRouteBuilder = true;

                                return result;
                        }
                        }
                        catch (Exception ex)
                        {
                            throw new ApplicationException(string.Format("Process template activation failed for action {0}.", curActionDO.Name), ex);
                        }
                    }
                }

                if (result.Status != "validation_error")
                {
                uow.RouteRepository.GetByKey(curPlanId).RouteState = RouteState.Active;
                uow.SaveChanges();
            }
            }

            return result;
        }

        /// <summary>
        /// After receiving response from terminals for activate action call, checks for existing validation errors on some controls
        /// </summary>
        /// <param name="curActionDTO"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private bool CheckForExistingValidationErrors(ActionDTO curActionDTO, out string errorMessage)
        {
            errorMessage = string.Empty;

            var crateStorage = _crate.GetStorage(curActionDTO);

            var configControls = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().ToList();
            //search for an error inside the config controls and return back if exists
            foreach (var controlGroup in configControls)
            {
                var control = controlGroup.Controls.FirstOrDefault(x => !string.IsNullOrEmpty(x.ErrorMessage));
                if (control != null)
                {
                    //here show only the first error as an issue to redirect back the user to the plan builder
                    errorMessage = string.Format("There was a problem with the configuration of the action '{0}': {1}",
                        curActionDTO.Name, control.ErrorMessage);
                    return true;
                }
            }

            return false;
        }

        public async Task<string> Deactivate(Guid curPlanId)
        {
            string result = "no action";

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = uow.RouteRepository.GetByKey(curPlanId);

                foreach (SubrouteDO template in plan.Subroutes)
                {
                    var activities = EnumerateActivityTree<ActionDO>(template);
                    foreach (var curActionDO in activities)
                    {
                        try
                        {
                            var resultD = await _action.Deactivate(curActionDO);

                            result = "success";
                        }
                        catch (Exception ex)
                        {
                            throw new ApplicationException("Process template activation failed.", ex);
                        }
                    }
                }

                uow.RouteRepository.GetByKey(curPlanId).RouteState = RouteState.Inactive;
                uow.SaveChanges();
            }

            return result;
        }


        // TODO: like some other methods, this assumes that there is only 1 action list in use. This is dangerous 
        //because the database allows N Activities.
        //we're waiting to reconcile this until we get some visibility into how the product is used by users
        //        public ActionListDO GetActionList(IUnitOfWork uow, int id)
        //        {
        //            // Get action list by process template first 
        //            var currentRoute = uow.RouteRepository.GetQuery().Where(pt => pt.Id == id).ToArray();
        //
        //            if (currentRoute.Length == 0)
        //            {
        //                return null;
        //            }
        //
        //            if (currentRoute.Length > 1)
        //            {
        //                throw new Exception(string.Format("More than one action list exists in processtemplate {0}", id));
        //            }
        //
        //            var startingRoute = currentRoute[0].StartingSubroute;
        //            if (startingRoute == null)
        //            {
        //                return null;
        //            }
        //
        //            // Get Activities related to the Route
        //            var curActionList = startingRoute.Activities.SingleOrDefault(al => al.ActionListType == ActionListType.Immediate);
        //            return curActionList;
        //
        //        }


        /// <summary>
        /// Returns all actions created within a Process Template.
        /// </summary>
        /// <param name="id">Process Template id.</param>
        /// <returns></returns>
        public IEnumerable<ActionDO> GetActions(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("id");
            }

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return EnumerateActivities<ActionDO>(uow.RouteRepository.GetByKey(id), false).ToArray();
            }
        }



        public IList<PlanDO> GetMatchingPlans(string userId, EventReportCM curEventReport)
        {
            List<PlanDO> routeSubscribers = new List<PlanDO>();
            if (String.IsNullOrEmpty(userId))
                throw new ArgumentNullException("Parameter UserId is null");
            if (curEventReport == null)
                throw new ArgumentNullException("Parameter Standard Event Report is null");

            //1. Query all RouteDO that are Active
            //2. are associated with the determined DockyardAccount
            //3. their first Activity has a Crate of  Class "Standard Event Subscriptions" which has inside of it an event name that matches the event name 
            //in the Crate of Class "Standard Event Reports" which was passed in.
            var curPlans = _dockyardAccount.GetActivePlans(userId).ToList();

            return MatchEvents(curPlans, curEventReport);
            //3. Get ActivityDO

        }


        public List<PlanDO> MatchEvents(List<PlanDO> curPlans, EventReportCM curEventReport)
        {
            List<PlanDO> subscribingRoutes = new List<PlanDO>();

            foreach (var curPlan in curPlans)
            {
                //get the 1st activity
                var actionDO = GetFirstActionWithEventSubscriptions(curPlan.Id);

                if (actionDO != null)
                {
                    var storage = _crate.GetStorage(actionDO.CrateStorage);

                    foreach (var subscriptionsList in storage.CrateContentsOfType<EventSubscriptionCM>())
                    {
                        bool hasEvents = subscriptionsList.Subscriptions
                            .Where(events => curEventReport.EventNames.ToUpper().Trim().Replace(" ", "").Contains(events.ToUpper().Trim().Replace(" ", "")))
                            .Any();

                        if (hasEvents)
                        {
                            subscribingRoutes.Add(curPlan);
                        }
                    }
                }
            }

            return subscribingRoutes;
        }

        private ActionDO GetFirstActionWithEventSubscriptions(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var root = uow.RouteNodeRepository.GetByKey(id);

                
                var queue = new Queue<RouteNodeDO>();
                queue.Enqueue(root);

                while (queue.Count > 0)
                {
                    var routeNode = queue.Dequeue();
                    var action = routeNode as ActionDO;

                    if (action != null)
                    {
                        var storage = _crate.GetStorage(action.CrateStorage);
                        if (storage.CratesOfType<EventSubscriptionCM>().Count() > 0)
                        {
                            return action;
                        }
                    }

                    if (routeNode.ChildNodes != null)
                    {
                        foreach (var childNode in routeNode.ChildNodes)
                        {
                            queue.Enqueue(childNode);
                        }
                    }
                }

                return null;
            }
        }

        public RouteNodeDO GetFirstActivity(Guid curPlanId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return GetInitialActivity(uow, uow.RouteRepository.GetByKey(curPlanId));
            }
        }

        public RouteNodeDO GetInitialActivity(IUnitOfWork uow, PlanDO curPlan)
        {
            return EnumerateActivities<RouteNodeDO>(curPlan, false).OrderBy(a => a.Ordering).FirstOrDefault();
        }

        public RouteNodeDO GetRootActivity(IUnitOfWork uow, PlanDO curPlan)
        {
            return curPlan.StartingSubroute as RouteNodeDO;
            //return EnumerateActivities<RouteNodeDO>(curPlan, false).OrderBy(a => a.Ordering).FirstOrDefault();
        }

        public PlanDO GetPlan(ActionDO action)
        {
            var root = action.ParentRouteNode;

            while (root != null)
            {
                if (root is PlanDO)
                {
                    return (PlanDO)root;
                }

                root = root.ParentRouteNode;
            }

            return null;
        }

        public PlanDO Copy(IUnitOfWork uow, PlanDO plan, string name)
        {
            var root = (PlanDO)plan.Clone();
            root.Id = Guid.NewGuid();
            root.Name = name;
            uow.RouteNodeRepository.Add(root);

            var queue = new Queue<Tuple<RouteNodeDO, Guid>>();
            queue.Enqueue(new Tuple<RouteNodeDO, Guid>(root, plan.Id));

            while (queue.Count > 0)
            {
                var routeTuple = queue.Dequeue();
                var routeNode = routeTuple.Item1;
                var sourceRouteNodeId = routeTuple.Item2;

                var sourceChildren = uow.RouteNodeRepository
                    .GetQuery()
                    .Where(x => x.ParentRouteNodeId == sourceRouteNodeId)
                    .ToList();

                foreach (var sourceChild in sourceChildren)
                {
                    var childCopy = sourceChild.Clone();
                    childCopy.Id = Guid.NewGuid();
                    childCopy.ParentRouteNode = routeNode;
                    routeNode.ChildNodes.Add(childCopy);

                    uow.RouteNodeRepository.Add(childCopy);

                    queue.Enqueue(new Tuple<RouteNodeDO, Guid>(childCopy, sourceChild.Id));
                }
            }

            return root;
        }

        /// <summary>
        /// New Process object
        /// </summary>
        /// <param name="planId"></param>
        /// <param name="envelopeId"></param>
        /// <returns></returns>
        public ContainerDO Create(IUnitOfWork uow, Guid planId, Crate curEvent)
        {
            var containerDO = new ContainerDO { Id = Guid.NewGuid() };

            var curPlan = uow.RouteRepository.GetByKey(planId);
            if (curPlan == null)
                throw new ArgumentNullException("planId");
            containerDO.Plan = curPlan;

            containerDO.Name = curPlan.Name;
            containerDO.ContainerState = ContainerState.Unstarted;

            if (curEvent != null)
            {
                using (var updater = _crate.UpdateStorage(() => containerDO.CrateStorage))
                {
                    updater.CrateStorage.Add(curEvent);
                }
            }

            containerDO.CurrentRouteNode = GetRootActivity(uow, curPlan);

            uow.ContainerRepository.Add(containerDO);
            uow.SaveChanges();

            //then create process node
            var subrouteId = containerDO.Plan.StartingSubroute.Id;

            var curProcessNode = _processNode.Create(uow, containerDO.Id, subrouteId, "process node");
            containerDO.ProcessNodes.Add(curProcessNode);

            uow.SaveChanges();
            EventManager.ContainerCreated(containerDO);

            return containerDO;
        }

        private async Task<ContainerDO> Run(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            if (curContainerDO.ContainerState == ContainerState.Failed || curContainerDO.ContainerState == ContainerState.Completed)
            {
                throw new ApplicationException("Attempted to Launch a Process that was Failed or Completed");
            }

            try
            {
                await _container.Run(uow, curContainerDO);
                return curContainerDO;
            }
            catch
            {
                curContainerDO.ContainerState = ContainerState.Failed;
                throw;
            }
            finally
            {
                //TODO is this necessary? let's leave it as it is
                /*
                curContainerDO.CurrentRouteNode = null;
                curContainerDO.NextRouteNode = null;
                 * */
                uow.SaveChanges();
            }
        }

        public async Task<ContainerDO> Run(PlanDO curPlan, Crate curEvent)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curContainerDO = Create(uow, curPlan.Id, curEvent);
                return await Run(uow, curContainerDO);
            }
        }

        public async Task<ContainerDO> Continue(Guid containerId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curContainerDO = uow.ContainerRepository.GetByKey(containerId);
                if (curContainerDO.ContainerState != ContainerState.Pending)
                {
                    throw new ApplicationException("Attempted to Continue a Process that wasn't pending");
                }
                //continue from where we left
                return await Run(uow, curContainerDO);
            }
        }

        /// <summary>
        /// The function add/removes items on the current collection 
        /// so that they match the items on the new collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionToUpdate"></param>
        /// <param name="sourceCollection"></param>
        /* public void MakeCollectionEqual<T>(IUnitOfWork uow, IList<T> collectionToUpdate, IList<T> sourceCollection) where T : class
        {
            List<T> itemsToAdd = new List<T>();
            List<T> itemsToRemove = new List<T>();
            bool found;

            foreach (T entity in collectionToUpdate)
            {
                found = false;
                foreach (T entityToCompare in sourceCollection)
                {
                    if (((IEquatable<T>)entity).Equals(entityToCompare))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    itemsToRemove.Add(entity);
                }
            }
            itemsToRemove.ForEach(e => uow.Db.Entry(e).State = EntityState.Deleted);

            foreach (T entity in sourceCollection)
            {
                found = false;
                foreach (T entityToCompare in collectionToUpdate)
                {
                    if (((IEquatable<T>)entity).Equals(entityToCompare))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    itemsToAdd.Add(entity);
                }
            }
            itemsToAdd.ForEach(i => collectionToUpdate.Add(i));


            //identify deleted items and remove them from the collection
            //collectionToUpdate.Except(sourceCollection).ToList().ForEach(s => collectionToUpdate.Remove(s));
            //identify added items and add them to the collection
            //sourceCollection.Except(collectionToUpdate).ToList().ForEach(s => collectionToUpdate.Add(s));
        }
        */
    }
}