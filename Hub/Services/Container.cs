using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Constants;
using Data.Crates;
using Data.Interfaces.Manifests;
using Hub.Exceptions;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Interfaces;
using Hub.Managers;

namespace Hub.Services
{
    public class Container : Hub.Interfaces.IContainer
    {

        // Declarations

        private readonly IActivity _activity;
        private readonly ICrateManager _crate;

        public Container()
        {
            _activity = ObjectFactory.GetInstance<IActivity>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
        }

        public List<ContainerDO> LoadContainers(IUnitOfWork uow, PlanDO plan)
        {
            return uow.ContainerRepository.GetQuery().Where(x => x.PlanId == plan.Id).ToList();
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
            await plan.Run(uow, planDO, payloadCrates);
        }

        // Executes node is passed if it is an activity
        private async Task<ICrateStorage> ExecuteNode(IUnitOfWork uow, PlanNodeDO currentNode, ContainerDO container, ActivityExecutionMode mode)
        {
            var currentActivity = currentNode as ActivityDO;

            if (currentActivity == null)
            {
                return null;
            }
          
            var payload = await _activity.Run(uow, currentActivity, mode, container);

            if (payload != null)
            {
                return _crate.FromDto(payload.CrateStorage);
            }

            return null;
        }

        //this method is for copying payload that activity returns with container's payload.
        private void SyncPayload(ICrateStorage activityPayloadStorage, IUpdatableCrateStorage containerStorage, Stack<OperationalStateCM.StackFrame> callStack)
        {
            if (activityPayloadStorage == null)
            {
                return;
            }

            containerStorage.Replace(activityPayloadStorage);
            var operationalState = containerStorage.CrateContentsOfType<OperationalStateCM>().Single();

            // just replace call stack with what we are using while running container. Activity can't change call stack and even if it happens we wan't to discard such action
            operationalState.CallStack = callStack;
        }


        private bool ProcessOpCodes(ActivityResponseDTO activityResponse, ContainerDO container, OperationalStateCM.ActivityExecutionPhase activityExecutionPhase, OperationalStateCM.StackFrame topFrame)
        {
            //activityResponse.

            return true;
        }
        
        // See https://maginot.atlassian.net/wiki/display/DDW/New+container+execution+logic for details
        public async Task Run(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            if (curContainerDO == null)
            {
                throw new ArgumentNullException("ContainerDO is null");
            }
            
            var storage = _crate.GetStorage(curContainerDO.CrateStorage);
            var operationalState = storage.CrateContentsOfType<OperationalStateCM>().Single();

            if (operationalState == null)
            {
                throw new InvalidOperationException("Can't run container without properly intialized OperationalStateCM crate.");
            }

            // this is something that can't be changed by running activities. So we store current call stack for the entire run session.
            var callStack = operationalState.CallStack;

            if (callStack.Count == 0)
            {
                throw new InvalidOperationException("Current container has empty call stack that usually means that execution is completed. We can't run it again.");
            }

            curContainerDO.ContainerState = ContainerState.Executing;
            uow.SaveChanges();

            while (callStack.Count > 0)
            {
                var topFrame = callStack.Peek();
                var currentNode = uow.PlanRepository.GetById<PlanNodeDO>(topFrame.NodeId);

                try
                {
                    try
                    {
                        using (var payloadStorage = _crate.UpdateStorage(() => curContainerDO.CrateStorage))
                        {
                            ICrateStorage activityPayloadStorage;

                            if (topFrame.CurrentActivityExecutionPhase == OperationalStateCM.ActivityExecutionPhase.WasNotExecuted)
                            {
                                activityPayloadStorage = await ExecuteNode(uow, currentNode, curContainerDO, ActivityExecutionMode.InitialRun);

                                SyncPayload(activityPayloadStorage, payloadStorage, callStack);

                                topFrame.CurrentActivityExecutionPhase = OperationalStateCM.ActivityExecutionPhase.ProcessingChildren;

                                // process op codes
                                if (!ProcessOpCodes(operationalState.CurrentActivityResponse, curContainerDO, OperationalStateCM.ActivityExecutionPhase.WasNotExecuted, topFrame))
                                {
                                    break;
                                }

                                continue;
                            }

                            var currentChild = topFrame.CurrentChildId != null ? uow.PlanRepository.GetById<PlanNodeDO>(topFrame.CurrentChildId.Value) : null;
                            var nextChild = currentChild != null ? currentNode.ChildNodes.FirstOrDefault(x => x.Ordering > currentChild.Ordering)
                                : currentNode.ChildNodes.FirstOrDefault();

                            // if there is a child that has not being executed yet - mark it for execution by pushing to stack
                            if (nextChild != null)
                            {
                                callStack.Push(new OperationalStateCM.StackFrame
                                {
                                    NodeId = nextChild.Id
                                });

                                topFrame.CurrentChildId = nextChild.Id;
                            }
                            // or run current activity in ReturnFromChildren mode
                            else
                            {
                                if (currentNode.ChildNodes.Count > 0)
                                {
                                    activityPayloadStorage = await ExecuteNode(uow, currentNode, curContainerDO, ActivityExecutionMode.ReturnFromChildren);

                                    SyncPayload(activityPayloadStorage, payloadStorage, callStack);
                                }

                                callStack.Pop();

                                // process op codes
                                if (!ProcessOpCodes(operationalState.CurrentActivityResponse, curContainerDO, OperationalStateCM.ActivityExecutionPhase.WasNotExecuted, topFrame))
                                {
                                    break;
                                }
                            }
                        }
                    }
                    finally
                    {
                        uow.SaveChanges();
                    }
                }
                catch (ErrorResponseException e)
                {
                    throw new ActivityExecutionException(e.ContainerDTO, Mapper.Map<ActivityDO, ActivityDTO>((ActivityDO)currentNode), e.Message, e);
                }
                catch (Exception e)
                {
                    var curActivity = currentNode as ActivityDO;

                    if (curActivity != null)
                    {
                        throw new ActivityExecutionException(Mapper.Map<ContainerDO, ContainerDTO>(curContainerDO), 
                                                             Mapper.Map<ActivityDO, ActivityDTO>(curActivity), string.Empty, e);
                    }

                    throw;
                }
            }

            if (curContainerDO.ContainerState == ContainerState.Executing)
            {
                curContainerDO.ContainerState = ContainerState.Completed;
                uow.SaveChanges();
            }
        }

        /*
            var activityResponseDTO = operationalState.CurrentActivityResponse;
    
                        //extract ActivityResponse type from result
                            ActivityResponse activityResponse = ActivityResponse.Null;
                            if (activityResponseDTO != null)
                            {
                                Enum.TryParse(activityResponseDTO.Type, out activityResponse);
                            }

                            ResponseMessageDTO responseMessage;

                            switch (activityResponse)
                            {
                                case ActivityResponse.ExecuteClientActivity:
                                case ActivityResponse.Success:
                                case ActivityResponse.ReProcessChildren:
                                case ActivityResponse.Null: //let's assume this is success for now


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
                                    actionState = ActivityExecutionMode.InitialRun;
                                    activityResponseDTO.TryParseResponseMessageDTO(out responseMessage);
                                    curContainerDO.CurrentPlanNodeId = Guid.Parse((string)responseMessage.Details);
                                    continue;

                                case ActivityResponse.JumpToSubplan:
                                    actionState = ActivityExecutionMode.InitialRun;
                                    activityResponseDTO.TryParseResponseMessageDTO(out responseMessage);
                                    var subplanId = Guid.Parse((string)responseMessage.Details);
                                    curContainerDO.CurrentPlanNodeId = GetFirstActivityOfSubplan(uow, curContainerDO, subplanId);
                                    continue;

                                case ActivityResponse.RequestLaunch:
                                    activityResponseDTO.TryParseResponseMessageDTO(out responseMessage);
                                    var planId = Guid.Parse((string)responseMessage.Details);
                                    //hmm what to do now
                                    await LoadAndRunPlan(uow, curContainerDO, planId);
                                    break;

                                default:
                                    throw new Exception("Unknown activity state on activity with id " + curContainerDO.CurrentPlanNodeId);
                            }

                            operationalState.CurrentActivityResponse = ActivityResponseDTO.Create(ActivityResponse.Null);
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
        */

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
    }
}
