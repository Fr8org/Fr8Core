using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Constants;
using Data.Crates;
using Data.Interfaces.Manifests;
using Hub.Exceptions;
using Newtonsoft.Json;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Interfaces;
using Data.Infrastructure;
using Data.Interfaces.DataTransferObjects.Helpers;
using Hub.Managers;

namespace Hub.Services
{
    public class Container : Hub.Interfaces.IContainer
    {

        // Declarations

        private readonly IPlanNode _activity;
        private readonly ICrateManager _crate;

        public Container()
        {
            _activity = ObjectFactory.GetInstance<IPlanNode>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
        }

        private void AddOperationalStateCrate(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            using (var crateStorage = _crate.UpdateStorage(() => curContainerDO.CrateStorage))
            {
                var operationalStatus = new OperationalStateCM();
                var operationsCrate = Crate.FromContent("Operational Status", operationalStatus);
                crateStorage.Add(operationsCrate);
            }

            uow.SaveChanges();
        }

        private ActivityResponseDTO GetCurrentActivityResponse(ContainerDO curContainerDO)
        {
            var storage = _crate.GetStorage(curContainerDO.CrateStorage);
            var operationalState = storage.CrateContentsOfType<OperationalStateCM>().Single();
            return operationalState.CurrentActivityResponse;
        }

        private void ResetActivityResponse(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            using (var crateStorage = _crate.UpdateStorage(() => curContainerDO.CrateStorage))
            {
                var operationalState = crateStorage.CrateContentsOfType<OperationalStateCM>().Single();

                bool isRequestSuspend = operationalState.CurrentActivityResponse != null
                    && operationalState.CurrentActivityResponse.Type == ActivityResponse.RequestSuspend.ToString();

                if (!isRequestSuspend)
                {
                    operationalState.CurrentActivityResponse = ActivityResponseDTO.Create(ActivityResponse.Null);
                }
            }

            uow.SaveChanges();
        }

        public List<ContainerDO> LoadContainers(IUnitOfWork uow, PlanDO plan)
        {
            return uow.ContainerRepository.GetQuery().Where(x => x.PlanId == plan.Id).ToList();
        }


        /* 
        *          a
        *       b     c 
        *     d   E  f  g  
        * 
        * 
        * We traverse this tree in this order a-b-d-E-b-c-f-g-c-a-NULL 
        */
        /// <summary>
        /// Moves to next Plan and returns action state of this new plan
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="curContainerDO"></param>
        /// <param name="skipChildren"></param>
        private ActivityState MoveToNextPlan(IUnitOfWork uow, ContainerDO curContainerDO, bool skipChildren)
        {
            var state = ActivityState.InitialRun;
            var currentNode = uow.PlanRepository.GetById<PlanNodeDO>(curContainerDO.CurrentPlanNodeId);
            
            // we need this to make tests wokring. If we leave currentplannode not null, MockDB will restore CurrentPlanNodeId. 
            // EF should just igone navigational porperty null value if corresponding foreign key is not null.
            curContainerDO.CurrentPlanNode = null;

            if (skipChildren || currentNode.ChildNodes.Count == 0)
            {
                var nextSibling = _activity.GetNextSibling(currentNode);
                if (currentNode is SubPlanDO && nextSibling is SubPlanDO)
                {
                    //we should never jump between subplans unless explicitly told
                    //let's stop here
                    curContainerDO.CurrentPlanNodeId = null;
                    state = ActivityState.ReturnFromChildren;
                }
                else
                { 
                    if (nextSibling == null)
                    {
                        curContainerDO.CurrentPlanNodeId = currentNode.ParentPlanNode?.Id;
                        state = ActivityState.ReturnFromChildren;
                    }
                    else
                    {
                        curContainerDO.CurrentPlanNodeId = nextSibling.Id;
                    }
                }
            }
            else
            {
                var firstChild = _activity.GetFirstChild(currentNode);
                curContainerDO.CurrentPlanNodeId = firstChild.Id;
            }

            uow.SaveChanges();
            return state;
        }

        /// <summary>
        /// Run current action and return it's response
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="curContainerDO"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private async Task<ActivityResponseDTO> ProcessActivity(IUnitOfWork uow, ContainerDO curContainerDO, ActivityState state)
        {
            ResetActivityResponse(uow, curContainerDO);
            await _activity.Process(curContainerDO.CurrentPlanNodeId.Value, state, curContainerDO);
            return GetCurrentActivityResponse(curContainerDO);
        }

        private bool ShouldSkipChildren(ContainerDO curContainerDO, ActivityState state, ActivityResponse response)
        {
            //first let's check if there is a child action related response
            if (response == ActivityResponse.SkipChildren)
            {
                return true;
            }
            else if (response == ActivityResponse.ReProcessChildren)
            {
                return false;
            }

            //otherwise we will assume this is a regular action
            //so we will process it's children once

            if (state == ActivityState.InitialRun)
            {
                return false;
            }
            else if (state == ActivityState.ReturnFromChildren)
            {
                return true;
            }

            throw new Exception("This shouldn't happen");
        }

        private bool HasOperationalStateCrate(ContainerDO curContainerDO)
        {
            
            var storage = _crate.GetStorage(curContainerDO.CrateStorage);
            var operationalState = storage.CrateContentsOfType<OperationalStateCM>().FirstOrDefault();
            return operationalState != null;
        }

        private Guid? GetFirstActivityOfSubplan(IUnitOfWork uow, ContainerDO curContainerDO, Guid subplanId)
        {
            var subplan = uow.PlanRepository.GetById<PlanDO>(curContainerDO.PlanId).SubPlans.FirstOrDefault(s => s.Id == subplanId);
            return subplan?.ChildNodes.OrderBy(c => c.Ordering).FirstOrDefault()?.Id;
        }

        private async Task LoadAndRunPlan(IUnitOfWork uow, ContainerDO curContainerDO, ActivityResponseDTO activityResponseDTO)
        {
            ResponseMessageDTO responseMessage;
            activityResponseDTO.TryParseResponseMessageDTO(out responseMessage);
            var addPlanId = Guid.Parse((string)responseMessage.Details);
                
            await LoadAndRunPlan(uow, curContainerDO, addPlanId);
        }

        private async Task LoadAndRunPlan(IUnitOfWork uow, ContainerDO curContainerDO, Guid planId)
        {
            var plan = ObjectFactory.GetInstance<IPlan>();
            var planDO = uow.PlanRepository.GetById<PlanDO>(planId);
            var freshContainer = uow.ContainerRepository.GetByKey(curContainerDO.Id);

            var crateStorage = _crate.GetStorage(freshContainer.CrateStorage);
            var operationStateCrate = crateStorage.CrateContentsOfType<OperationalStateCM>().Single();
            operationStateCrate.CurrentActivityResponse = ActivityResponseDTO.Create(ActivityResponse.Null);
            operationStateCrate.History.Add(new OperationalStateCM.HistoryElement { Description = "Launch Triggered by Container ID "+ curContainerDO.Id });

            var payloadCrates = crateStorage.AsEnumerable().ToArray();
            plan.Enqueue(planDO.Id, payloadCrates);
        }

        public async Task Run(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            if (curContainerDO == null)
            {
                throw new ArgumentNullException("ContainerDO is null");
            }
            
            try
            {
                //if payload already has operational state create we shouldn't create another
                if (!HasOperationalStateCrate(curContainerDO))
                {
                    AddOperationalStateCrate(uow, curContainerDO);
                }

                curContainerDO.ContainerState = ContainerState.Executing;
                uow.SaveChanges();

                if (curContainerDO.CurrentPlanNodeId == null)
                {
                    throw new ArgumentNullException("CurrentActivity is null. Cannot execute CurrentActivity");
                }

                var actionState = ActivityState.InitialRun;
                while (curContainerDO.CurrentPlanNodeId != null)
                {
                    var activityResponseDTO = await ProcessActivity(uow, curContainerDO, actionState);

                    //extract ActivityResponse type from result
                    ActivityResponse activityResponse = ActivityResponse.Null;
                    if (activityResponseDTO != null)
                        Enum.TryParse(activityResponseDTO.Type, out activityResponse);

                    ResponseMessageDTO responseMessage;

                    switch (activityResponse)
                    {
                        case ActivityResponse.ExecuteClientActivity:
                        case ActivityResponse.Success:
                        case ActivityResponse.ReProcessChildren:
                        case ActivityResponse.Null://let's assume this is success for now


                            break;
                        case ActivityResponse.RequestSuspend:
                            curContainerDO.ContainerState = ContainerState.Pending;
                            return;

                        case ActivityResponse.Error:
                            //TODO retry activity execution until 3 errors??    
                            //so we are able to show the specific error that is embedded inside the container we are sending back that container to client
                            ErrorDTO error = activityResponseDTO.TryParseErrorDTO(out error) ? error : null;
                            throw new ErrorResponseException(Mapper.Map<ContainerDO, ContainerDTO>(curContainerDO), error?.Message);
                        case ActivityResponse.RequestTerminate:
                            //FR-2163 - If action response requests for termination, we make the container as Completed to avoid unwanted errors.
                            curContainerDO.ContainerState = ContainerState.Completed;
                            EventManager.ProcessingTerminatedPerActivityResponse(curContainerDO, ActivityResponse.RequestTerminate);

                            return;

                        case ActivityResponse.JumpToActivity:
                            actionState = ActivityState.InitialRun;
                            activityResponseDTO.TryParseResponseMessageDTO(out responseMessage);
                            curContainerDO.CurrentPlanNodeId = Guid.Parse((string)responseMessage.Details);
                            continue;

                        case ActivityResponse.LaunchAdditionalPlan:
                            //actionState = ActivityState.InitialRun;
                            await LoadAndRunPlan(uow, curContainerDO, activityResponseDTO);
                            continue;

                        case ActivityResponse.RequestLaunch:
                            //hmm what to do now
                            await LoadAndRunPlan(uow, curContainerDO, activityResponseDTO);
                            break;

                        default:
                            throw new Exception("Unknown activity state on activity with id " + curContainerDO.CurrentPlanNodeId);
                    }

                    var shouldSkipChildren = ShouldSkipChildren(curContainerDO, actionState, activityResponse);
                    actionState = MoveToNextPlan(uow, curContainerDO, shouldSkipChildren);
                }

                if (curContainerDO.ContainerState == ContainerState.Executing)
                {
                    curContainerDO.ContainerState = ContainerState.Completed;
                    uow.SaveChanges();

                }
            }
            catch (ErrorResponseException e)
            {
                var curActivityDTO = GetCurrentActivity(uow, curContainerDO);
                throw new ActivityExecutionException(e.ContainerDTO, curActivityDTO, e.Message, e);
            }            
            catch(Exception e)
            {
                var curActivityDTO = GetCurrentActivity(uow, curContainerDO);
                
                if (curActivityDTO != null)
                {
                    var curContainerDTO = Mapper.Map<ContainerDO, ContainerDTO>(curContainerDO);
                    throw new ActivityExecutionException(curContainerDTO, curActivityDTO, string.Empty, e);
                }
                else
                {
                    throw;
                }                
            }
        }

        // Return the Containers of current Account
        public IList<ContainerDO> GetByFr8Account(IUnitOfWork unitOfWork, Fr8AccountDO account, bool isAdmin = false, Guid? id = null)
        {
            if (account.Id == null)
                throw new ApplicationException("UserId must not be null");

            var containerRepository = unitOfWork.ContainerRepository.GetQuery();
            
      
            return (id == null
               ? containerRepository.Where(container => container.Plan.Fr8Account.Id == account.Id)
               : containerRepository.Where(container => container.Id == id && container.Plan.Fr8Account.Id == account.Id)).ToList();
        }

        private ActivityDTO GetCurrentActivity(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            if (curContainerDO == null || curContainerDO.CurrentPlanNodeId == null)
            {
                return null;
            }

            var curActivityId = curContainerDO.CurrentPlanNodeId.Value;
            var curPlanNodeDO = uow.PlanRepository.GetById<PlanNodeDO>(curActivityId);
            var curActivityDO = curPlanNodeDO as ActivityDO;

            if (curActivityDO != null)
            {
                return Mapper.Map<ActivityDO, ActivityDTO>(curActivityDO);
            }
            else
            {
                return null;
            }
        }
    }
}
