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
using Data.Constants;
using Data.Crates;
using Data.Infrastructure;
using Data.Interfaces.DataTransferObjects.Helpers;

namespace Hub.Services
{
    public class Plan : IPlan
    {
        // private readonly IProcess _process;
        private readonly InternalInterface.IContainer _container;
        private readonly Fr8Account _dockyardAccount;
        private readonly IActivity _activity;
        private readonly ICrateManager _crate;
        private readonly ISecurityServices _security;
        private readonly IProcessNode _processNode;

        public Plan(InternalInterface.IContainer container, Fr8Account dockyardAccount, IActivity activity,
            ICrateManager crate, ISecurityServices security, IProcessNode processNode)
        {
            _container = container;
            _dockyardAccount = dockyardAccount;
            _activity = activity;
            _crate = crate;
            _security = security;
            _processNode = processNode;
        }

        public IList<PlanDO> GetForUser(IUnitOfWork unitOfWork, Fr8AccountDO account, bool isAdmin = false,
            Guid? id = null, int? status = null)
        {
            var queryableRepo = unitOfWork.PlanRepository.GetPlanQueryUncached()
                .Where(x => x.Visibility == PlanVisibility.Standard);

            queryableRepo = (id == null
                ? queryableRepo.Where(pt => pt.Fr8Account.Id == account.Id)
                : queryableRepo.Where(pt => pt.Id == id && pt.Fr8Account.Id == account.Id));
            return (status == null
                ? queryableRepo
                : queryableRepo.Where(pt => pt.RouteState == status)).ToList();

        }

        public IList<PlanDO> GetByName(IUnitOfWork uow, Fr8AccountDO account, string name)
        {
            return
                uow.PlanRepository.GetPlanQueryUncached()
                    .Where(r => r.Fr8Account.Id == account.Id && r.Name == name)
                    .ToList();
        }

        public void CreateOrUpdate(IUnitOfWork uow, PlanDO submittedPlan, bool updateChildEntities)
        {
            if (submittedPlan.Id == Guid.Empty)
            {
                submittedPlan.Id = Guid.NewGuid();
                submittedPlan.RouteState = RouteState.Inactive;
                submittedPlan.Fr8Account = _security.GetCurrentAccount(uow);

                submittedPlan.ChildNodes.Add(new SubrouteDO(true)
                {
                    Id = Guid.NewGuid()
                });

                uow.PlanRepository.Add(submittedPlan);
            }
            else
            {
                var curPlan = uow.PlanRepository.GetById<PlanDO>(submittedPlan.Id);

                if (curPlan == null)
                {
                    throw new EntityNotFoundException();
                }

                curPlan.Name = submittedPlan.Name;
                curPlan.Description = submittedPlan.Description;
            }
        }

        public PlanDO Create(IUnitOfWork uow, string name)
        {
            var plan = new PlanDO
            {
                Id = Guid.NewGuid(),
                Name = name,
                Fr8Account = _security.GetCurrentAccount(uow),
                RouteState = RouteState.Inactive
            };

            uow.PlanRepository.Add(plan);

            return plan;
        }

        public void Delete(IUnitOfWork uow, Guid id)
        {
            var plan = uow.PlanRepository.GetById<PlanDO>(id);

            if (plan == null)
            {
                throw new EntityNotFoundException<PlanDO>(id);
            }

            plan.RouteState = RouteState.Deleted;

            //Route deletion will only update its RouteState = Deleted
            foreach (var container in _container.LoadContainers(uow, plan))
            {
                container.ContainerState = ContainerState.Deleted;
            }
        }


        public async Task<ActivateActivitiesDTO> Activate(Guid curPlanId, bool routeBuilderActivate)
        {
            var result = new ActivateActivitiesDTO
            {
                Status = "no action",
                ActivitiesCollections = new List<ActivityDTO>()
            };

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = uow.PlanRepository.GetById<PlanDO>(curPlanId);

                foreach (var curActionDO in plan.GetDescendants().OfType<ActivityDO>())
                {
                    try
                    {
                        var resultActivate = await _activity.Activate(curActionDO);

                        ContainerDTO errorContainerDTO;
                        result.Status = "success";

                        var validationErrorChecker = CheckForExistingValidationErrors(resultActivate, out errorContainerDTO);
                        if (validationErrorChecker)
                        {
                            result.Status = "validation_error";
                            result.Container = errorContainerDTO;
                        }

                        //if the activate call is comming from the Route Builder just render again the action group with the errors
                        if (routeBuilderActivate)
                        {
                            result.ActivitiesCollections.Add(resultActivate);
                        }
                        else if (validationErrorChecker)
                        {
                            //if the activate call is comming from the Routes List then show the first error message and redirect to plan builder 
                            //so the user could fix the configuration
                            result.RedirectToRouteBuilder = true;

                            return result;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException(string.Format("Process template activation failed for action {0}.", curActionDO.Label), ex);
                    }
                }


                if (result.Status != "validation_error")
                {
                    plan.RouteState = RouteState.Active;
                    uow.SaveChanges();
                }
            }

            return result;
        }

        /// <summary>
        /// After receiving response from terminals for activate action call, checks for existing validation errors on some controls
        /// </summary>
        /// <param name="curActivityDTO"></param>
        /// <param name="containerDTO">Use containerDTO as a wrapper for the Error with proper ActivityResponse and error DTO</param>
        /// <returns></returns>
        private bool CheckForExistingValidationErrors(ActivityDTO curActivityDTO, out ContainerDTO containerDTO)
        {
            containerDTO = new ContainerDTO();

            var crateStorage = _crate.GetStorage(curActivityDTO);

            var configControls = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().ToList();
            //search for an error inside the config controls and return back if exists
            foreach (var controlGroup in configControls)
            {
                var control = controlGroup.Controls.FirstOrDefault(x => !string.IsNullOrEmpty(x.ErrorMessage));
                if (control != null)
                {
                    var containerDO = new ContainerDO() {CrateStorage = string.Empty, Name = string.Empty};

                    using (var tempCrateStorage = _crate.UpdateStorage(() => containerDO.CrateStorage))
                    {
                        var operationalState = new OperationalStateCM();
                        operationalState.CurrentActivityResponse = ActivityResponseDTO.Create(ActivityResponse.Error);
                        operationalState.CurrentActivityResponse.AddErrorDTO(ErrorDTO.Create(control.ErrorMessage, ErrorType.Generic, string.Empty, null, curActivityDTO.ActivityTemplate.Name, curActivityDTO.ActivityTemplate.Terminal.Name));

                        var operationsCrate = Crate.FromContent("Operational Status", operationalState);
                        tempCrateStorage.Add(operationsCrate);
                    }

                    containerDTO = Mapper.Map<ContainerDTO>(containerDO);
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
                var plan = uow.PlanRepository.GetById<PlanDO>(curPlanId);


                foreach (var curActionDO in plan.GetDescendants().OfType<ActivityDO>())
                {
                    try
                    {
                        await _activity.Deactivate(curActionDO);
                        result = "success";
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException("Process template activation failed.", ex);
                    }
                }


                plan.RouteState = RouteState.Inactive;
                uow.SaveChanges();
            }

            return result;
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
                var actionDO = GetFirstActivityWithEventSubscriptions(curPlan.Id);

                if (actionDO != null)
                {
                    var storage = _crate.GetStorage(actionDO.CrateStorage);

                    foreach (var subscriptionsList in storage.CrateContentsOfType<EventSubscriptionCM>())
                    {
                        var manufacturer = subscriptionsList.Manufacturer;
                        bool hasEvents;
                        if (string.IsNullOrEmpty(manufacturer) || string.IsNullOrEmpty(curEventReport.Manufacturer))
                        {
                            hasEvents = subscriptionsList.Subscriptions.Any(events => curEventReport.EventNames.ToUpper().Trim().Replace(" ", "").Contains(events.ToUpper().Trim().Replace(" ", "")));
                        }
                        else
                        {
                            hasEvents = subscriptionsList.Subscriptions.Any(events => curEventReport.Manufacturer == manufacturer &&
                                curEventReport.EventNames.ToUpper().Trim().Replace(" ", "").Contains(events.ToUpper().Trim().Replace(" ", "")));
                        }

                        if (hasEvents)
                        {
                            subscribingRoutes.Add(curPlan);
                        }
                    }
                }
            }

            return subscribingRoutes;
        }

        private ActivityDO GetFirstActivityWithEventSubscriptions(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var root = uow.PlanRepository.GetById<PlanDO>(id);
                if (root == null)
                {
                    return null;
                }

                return root.GetDescendantsOrdered().OfType<ActivityDO>().FirstOrDefault(
                    x =>
                    {
                        var storage = _crate.GetStorage(x.CrateStorage);
                        return storage.CratesOfType<EventSubscriptionCM>().Any();
                    });

            }
        }
        
        public PlanDO GetPlanByActivityId(IUnitOfWork uow, Guid id)
        {
            return (PlanDO)uow.PlanRepository.GetById<RouteNodeDO>(id).GetTreeRoot();
        }

        public PlanDO Copy(IUnitOfWork uow, PlanDO plan, string name)
        {
            throw new NotImplementedException();

//            var root = (PlanDO)plan.Clone();
//            root.Id = Guid.NewGuid();
//            root.Name = name;
//            uow.PlanRepository.Add(root);
//
//            var queue = new Queue<Tuple<RouteNodeDO, Guid>>();
//            queue.Enqueue(new Tuple<RouteNodeDO, Guid>(root, plan.Id));
//
//            while (queue.Count > 0)
//            {
//                var routeTuple = queue.Dequeue();
//                var routeNode = routeTuple.Item1;
//                var sourceRouteNodeId = routeTuple.Item2;
//
//                var sourceChildren = uow.
//                    .GetQuery()
//                    .Where(x => x.ParentRouteNodeId == sourceRouteNodeId)
//                    .ToList();
//
//                foreach (var sourceChild in sourceChildren)
//                {
//                    var childCopy = sourceChild.Clone();
//                    childCopy.Id = Guid.NewGuid();
//                    childCopy.ParentRouteNode = routeNode;
//                    routeNode.ChildNodes.Add(childCopy);
//
//                    uow.RouteNodeRepository.Add(childCopy);
//
//                    queue.Enqueue(new Tuple<RouteNodeDO, Guid>(childCopy, sourceChild.Id));
//                }
//            }
//
//            return root;
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

            var curPlan = uow.PlanRepository.GetById<PlanDO>(planId);
            if (curPlan == null)
                throw new ArgumentNullException("planId");

            containerDO.PlanId = curPlan.Id;
            containerDO.Name = curPlan.Name;
            containerDO.ContainerState = ContainerState.Unstarted;

            if (curEvent != null)
            {
                using (var crateStorage = _crate.UpdateStorage(() => containerDO.CrateStorage))
                {
                    crateStorage.Add(curEvent);
                }
            }

            containerDO.CurrentRouteNodeId = curPlan.StartingSubrouteId;

            uow.ContainerRepository.Add(containerDO);

            //then create process node
            var curProcessNode = _processNode.Create(uow, containerDO.Id, curPlan.StartingSubrouteId, "process node");
            containerDO.ProcessNodes.Add(curProcessNode);

            uow.SaveChanges();
            EventManager.ContainerCreated(containerDO);

            return containerDO;
        }

        private async Task<ContainerDO> Run(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            if (curContainerDO.ContainerState == ContainerState.Failed
                || curContainerDO.ContainerState == ContainerState.Completed)
            {
                throw new ApplicationException("Attempted to Launch a Process that was Failed or Completed");
            }

            try
            {
                await _container.Run(uow, curContainerDO);
                curContainerDO.ContainerState = ContainerState.Completed;
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
            return curContainerDO;
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
    }
}