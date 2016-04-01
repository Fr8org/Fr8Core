using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Constants;
using Data.Crates;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.DataTransferObjects.Helpers;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Exceptions;
using Hub.Interfaces;
using Hub.Managers;
using StructureMap;

namespace Hub.Services
{
    partial class Container
    {
        // class handling execution of the particular plan
        private class ExecutionSession
        {
            /**********************************************************************************/
            // Declarations
            /**********************************************************************************/

            private readonly IUnitOfWork _uow;
            private readonly Stack<OperationalStateCM.StackFrame> _callStack;
            private readonly ContainerDO _container;
            private OperationalStateCM _operationalState;
            private readonly IActivity _activity;
            private readonly ICrateManager _crate;
            
            /**********************************************************************************/
            // Functions
            /**********************************************************************************/

            public ExecutionSession(IUnitOfWork uow, Stack<OperationalStateCM.StackFrame> callStack, ContainerDO container, IActivity activity, ICrateManager crateManager)
            {
                _uow = uow;
                _callStack = callStack;
                _container = container;

                _activity = activity;
                _crate = crateManager;
            }

            /**********************************************************************************/

            private OperationalStateCM GetOperationalState(ICrateStorage crateStorage)
            {
                var operationalState = crateStorage.CrateContentsOfType<OperationalStateCM>().FirstOrDefault();

                if (operationalState == null)
                {
                    throw new Exception("OperationalState was not found within the container payload.");
                }

                return operationalState;
            }

            /**********************************************************************************/

            private OperationalStateCM.StackFrame PushFrame(Guid nodeId)
            {
                var node = _uow.PlanRepository.GetById<PlanNodeDO>(nodeId);
                string nodeName = "undefined";

                if (node is ActivityDO)
                {
                    nodeName = "Activity: " + ((ActivityDO) node).Label;
                }

                if (node is SubPlanDO)
                {
                    nodeName = "Subplan: " + ((SubPlanDO) node).Name;
                }

                var frame = new OperationalStateCM.StackFrame
                {
                    NodeId = nodeId,
                    NodeName = nodeName
                };

                _callStack.Push(frame);

                return frame;
            }

            /**********************************************************************************/
            // See https://maginot.atlassian.net/wiki/display/DDW/New+container+execution+logic for details
            public async Task Run()
            {
                while (_callStack.Count > 0)
                {
                    var topFrame = _callStack.Peek();
                    var currentNode = _uow.PlanRepository.GetById<PlanNodeDO>(topFrame.NodeId);

                    try
                    {
                        try
                        {
                            using (var payloadStorage = _crate.UpdateStorage(() => _container.CrateStorage))
                            {
                                _operationalState = GetOperationalState(payloadStorage);

                                // reset current activity response
                                _operationalState.CurrentActivityResponse = null;
                                // update container's payload
                                payloadStorage.Flush();

                                if (topFrame.CurrentActivityExecutionPhase == OperationalStateCM.ActivityExecutionPhase.WasNotExecuted)
                                {
                                    await ExecuteNode(currentNode, payloadStorage, ActivityExecutionMode.InitialRun);

                                    topFrame.CurrentActivityExecutionPhase = OperationalStateCM.ActivityExecutionPhase.ProcessingChildren;

                                    // process op codes
                                    if (!await ProcessOpCodes(_operationalState.CurrentActivityResponse, OperationalStateCM.ActivityExecutionPhase.WasNotExecuted, topFrame))
                                    {
                                        break;
                                    }

                                    continue;
                                }

                                var currentChild = topFrame.CurrentChildId != null ? _uow.PlanRepository.GetById<PlanNodeDO>(topFrame.CurrentChildId.Value) : null;
                                var nextChild = currentChild != null ? currentNode.ChildNodes.OrderBy(x => x.Ordering).FirstOrDefault(x => x.Ordering > currentChild.Ordering) : currentNode.ChildNodes.OrderBy(x => x.Ordering).FirstOrDefault();

                                // if there is a child that has not being executed yet - mark it for execution by pushing to stack
                                if (nextChild != null)
                                {
                                    PushFrame(nextChild.Id);
                                    topFrame.CurrentChildId = nextChild.Id;
                                }
                                // or run current activity in ReturnFromChildren mode
                                else
                                {
                                    if (currentNode.ChildNodes.Count > 0)
                                    {
                                        await ExecuteNode(currentNode, payloadStorage, ActivityExecutionMode.ReturnFromChildren);
                                    }

                                    _callStack.Pop();

                                    // process op codes
                                    if (!await ProcessOpCodes(_operationalState.CurrentActivityResponse, OperationalStateCM.ActivityExecutionPhase.ProcessingChildren, topFrame))
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        finally
                        {
                            _uow.SaveChanges();
                        }
                    }
                    catch (ErrorResponseException e)
                    {
                        throw new ActivityExecutionException(e.ContainerDTO, Mapper.Map<ActivityDO, ActivityDTO>((ActivityDO) currentNode), e.Message, e);
                    }
                    catch (Exception e)
                    {
                        var curActivity = currentNode as ActivityDO;

                        if (curActivity != null)
                        {
                            throw new ActivityExecutionException(Mapper.Map<ContainerDO, ContainerDTO>(_container), Mapper.Map<ActivityDO, ActivityDTO>(curActivity), string.Empty, e);
                        }

                        throw;
                    }
                }

                if (_container.ContainerState == ContainerState.Executing)
                {
                    _container.ContainerState = ContainerState.Completed;
                    _uow.SaveChanges();
                }
            }

            /**********************************************************************************/

            private Guid ExtractGuidParam(ActivityResponseDTO activityResponse)
            {
                ResponseMessageDTO responseMessage;

                if (!activityResponse.TryParseResponseMessageDTO(out responseMessage))
                {
                    throw new InvalidOperationException("Unable to parse op code parameter");
                }

                return  Guid.Parse((string)responseMessage.Details);
            }

            /**********************************************************************************/

            private async Task<bool> ProcessOpCodes(ActivityResponseDTO activityResponse, OperationalStateCM.ActivityExecutionPhase activityExecutionPhase, OperationalStateCM.StackFrame topFrame)
            {
                ActivityResponse opCode;

                if (activityResponse == null)
                {
                    return true;
                }

                if (!Enum.TryParse(activityResponse.Type, out opCode))
                {
                    return true;
                }

                switch (opCode)
                {
                    case ActivityResponse.Error:
                        ErrorDTO error = activityResponse.TryParseErrorDTO(out error) ? error : null;
                        throw new ErrorResponseException(Mapper.Map<ContainerDO, ContainerDTO>(_container), error?.Message);

                    case ActivityResponse.ExecuteClientActivity:
                        break;

                    case ActivityResponse.ShowDocumentation:
                        break;

                    case ActivityResponse.RequestLaunch:
                        await LoadAndRunPlan(ExtractGuidParam(activityResponse));
                        break;

                    case ActivityResponse.RequestTerminate:
                        _callStack.Clear();
                        EventManager.ProcessingTerminatedPerActivityResponse(_container, ActivityResponse.RequestTerminate);
                        return false;

                    case ActivityResponse.RequestSuspend:
                        _container.ContainerState = ContainerState.WaitingForTerminal;

                        if (activityExecutionPhase == OperationalStateCM.ActivityExecutionPhase.ProcessingChildren)
                        {
                            _callStack.Push(topFrame);
                        }

                        return false;

                    case ActivityResponse.SkipChildren:
                        if (activityExecutionPhase == OperationalStateCM.ActivityExecutionPhase.WasNotExecuted)
                        {
                            _callStack.Pop();
                        }
                        break;

                    case ActivityResponse.Jump:

                        if (activityExecutionPhase == OperationalStateCM.ActivityExecutionPhase.WasNotExecuted)
                        {
                            _callStack.Pop();
                        }

                        PushFrame(ExtractGuidParam(activityResponse));

                        break;

                    case ActivityResponse.JumpToActivity:
                    case ActivityResponse.JumpToSubplan:
                    case ActivityResponse.JumpFar:
                        var id = ExtractGuidParam(activityResponse);
                        _callStack.Clear();
                        PushFrame(id);
                        break;

                    case ActivityResponse.Call:
                        PushFrame(ExtractGuidParam(activityResponse));
                        break;

                    case ActivityResponse.Break:
                        if (activityExecutionPhase == OperationalStateCM.ActivityExecutionPhase.WasNotExecuted)
                        {
                            _callStack.Pop();
                        }

                        if (_callStack.Count > 0)
                        {
                            _callStack.Pop();
                        }
                        break;
                }

                return true;
            }

            /**********************************************************************************/

            private async Task LoadAndRunPlan(Guid planId)
            {
                var plan = ObjectFactory.GetInstance<IPlan>();
                var planDO = _uow.PlanRepository.GetById<PlanDO>(planId);
                var freshContainer = _uow.ContainerRepository.GetByKey(_container.Id);

                var crateStorage = _crate.GetStorage(freshContainer.CrateStorage);
                var operationStateCrate = crateStorage.CrateContentsOfType<OperationalStateCM>().Single();
                operationStateCrate.CurrentActivityResponse = ActivityResponseDTO.Create(ActivityResponse.Null);
                operationStateCrate.History.Add(new OperationalStateCM.HistoryElement {Description = "Launch Triggered by Container ID " + _container.Id});

                var payloadCrates = crateStorage.AsEnumerable().ToArray();

                await plan.Run(_uow, planDO, payloadCrates);
            }

            /**********************************************************************************/
            // Executes node is passed if it is an activity
            private async Task ExecuteNode(PlanNodeDO currentNode, IUpdatableCrateStorage payloadStorage, ActivityExecutionMode mode)
            {
                var currentActivity = currentNode as ActivityDO;

                if (currentActivity == null)
                {
                    return;
                }

                var payload = await _activity.Run(_uow, currentActivity, mode, _container);

                if (payload != null)
                {
                    var activityPayloadStroage = _crate.FromDto(payload.CrateStorage);

                    SyncPayload(activityPayloadStroage, payloadStorage);
                }
            }

            /**********************************************************************************/
            //this method is for copying payload that activity returns with container's payload.
            private void SyncPayload(ICrateStorage activityPayloadStorage, IUpdatableCrateStorage containerStorage)
            {
                if (activityPayloadStorage == null)
                {
                    return;
                }

                containerStorage.Replace(activityPayloadStorage);

                _operationalState = GetOperationalState(containerStorage);

                // just replace call stack with what we are using while running container. Activity can't change call stack and even if it happens we wan't to discard such action
                _operationalState.CallStack = _callStack;
            }

            /**********************************************************************************/
        }
    }
}
