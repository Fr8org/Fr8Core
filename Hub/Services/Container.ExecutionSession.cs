using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States;
using Fr8Data.Constants;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.DataTransferObjects.Helpers;
using Fr8Data.Managers;
using Fr8Data.Manifests;
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
#if DEBUG
            private const int MaxStackSize = 100;
#else
            private const int MaxStackSize = 250;
#endif

            private readonly IUnitOfWork _uow;
            private readonly OperationalStateCM.ActivityCallStack _callStack;
            private readonly ContainerDO _container;
            private OperationalStateCM _operationalState;
            private readonly IActivity _activity;
            private readonly ICrateManager _crate;
            
            /**********************************************************************************/
            // Functions
            /**********************************************************************************/

            public ExecutionSession(IUnitOfWork uow, OperationalStateCM.ActivityCallStack callStack, ContainerDO container, IActivity activity, ICrateManager crateManager)
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

            private void AddNodeForExecution(Guid nodeId)
            {
                var node = _uow.PlanRepository.GetById<PlanNodeDO>(nodeId);
                string nodeName = "undefined";

                if (node is ActivityDO)
                {
                    nodeName = "Activity: " + ((ActivityDO) node).Name;
                }

                if (node is SubPlanDO)
                {
                    nodeName = "Subplan: " + ((SubPlanDO) node).Name;
                }

                var frame = new OperationalStateCM.StackFrame
                {
                    NodeId = nodeId,
                    NodeName = nodeName,
                    LocalData = _operationalState.BypassData
                };

                _callStack.PushFrame(frame);
                _operationalState.BypassData = null;
            }

            /**********************************************************************************/
            // See https://maginot.atlassian.net/wiki/display/DDW/New+container+execution+logic for details
            public async Task Run()
            {
                while (_callStack.Count > 0)
                {
                    if (_callStack.Count > MaxStackSize)
                    {
                        throw new Exception($"Container execution stack overflow. Container: {_container.Id}. PlanId: {_container.PlanId}.");
                    }

                    var topFrame = _callStack.TopFrame;
                    var currentNode = _uow.PlanRepository.GetById<PlanNodeDO>(topFrame.NodeId);

                    if (currentNode == null)
                    {
                        throw new Exception($"PlanNode with id: {topFrame.NodeId} was not found. Container: {_container.Id}. PlanId: {_container.PlanId}.");
                    }

                    try
                    {
                        try
                        {
                            using (var payloadStorage = _crate.UpdateStorage(() => _container.CrateStorage))
                            {
                                _operationalState = GetOperationalState(payloadStorage);

                                _operationalState.CallStack = _callStack;
                                // reset current activity response
                                _operationalState.CurrentActivityResponse = null;
                                // update container's payload
                                payloadStorage.Flush();

                                if (topFrame.CurrentActivityExecutionPhase == OperationalStateCM.ActivityExecutionPhase.WasNotExecuted)
                                {
                                    await ExecuteNode(currentNode, payloadStorage, ActivityExecutionMode.InitialRun);

                                    topFrame.CurrentActivityExecutionPhase = OperationalStateCM.ActivityExecutionPhase.ProcessingChildren;

                                    // process op codes
                                    if (!ProcessActivityResponse(_operationalState.CurrentActivityResponse, OperationalStateCM.ActivityExecutionPhase.WasNotExecuted, topFrame))
                                    {
                                        break;
                                    }

                                    continue;
                                }

                                var nextChild = GetNextChildActivity(topFrame, currentNode);

                                // if there is a child that has not being executed yet - add it for execution by pushing to the call stack
                                if (nextChild != null)
                                {
                                    AddNodeForExecution(nextChild.Id);
                                    topFrame.CurrentChildId = nextChild.Id;
                                }
                                // or run current activity in ReturnFromChildren mode
                                else
                                {
                                    if (currentNode.ChildNodes.Count > 0)
                                    {
                                        await ExecuteNode(currentNode, payloadStorage, ActivityExecutionMode.ReturnFromChildren);
                                    }

                                    _callStack.RemoveTopFrame();

                                    // process op codes
                                    if (!ProcessActivityResponse(_operationalState.CurrentActivityResponse, OperationalStateCM.ActivityExecutionPhase.ProcessingChildren, topFrame))
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
                    catch (InvalidTokenRuntimeException)
                    {
                        throw;
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

                if (_container.State == State.Executing)
                {
                    _container.State = State.Completed;
                    _uow.SaveChanges();
                }
            }

            /**********************************************************************************/

            private PlanNodeDO GetNextChildActivity(OperationalStateCM.StackFrame topFrame, PlanNodeDO currentNode)
            {
                // get the currently processing child
                var currentChild = topFrame.CurrentChildId != null ? _uow.PlanRepository.GetById<PlanNodeDO>(topFrame.CurrentChildId.Value) : null;

                if (currentNode.ChildNodes == null)
                {
                    throw new NullReferenceException($"ChildNodes is null for node: {currentNode.Id}.");
                }

                // If we are already processing children of the currentNode, selecte the next one
                if (currentChild != null)
                {
                    return currentNode.ChildNodes.OrderBy(x => x.Ordering).FirstOrDefault(x => x.Ordering > currentChild.Ordering && x.Runnable);
                }

                // or, if we have not processed any child yet - select the first one if any
                return currentNode.ChildNodes.OrderBy(x => x.Ordering).FirstOrDefault(x => x.Runnable);
            }

            /**********************************************************************************/

            private Guid ExtractGuidParameter(ActivityResponseDTO activityResponse)
            {
                ResponseMessageDTO responseMessage;

                if (!activityResponse.TryParseResponseMessageDTO(out responseMessage))
                {
                    throw new InvalidOperationException("Unable to parse op code parameter");
                }

                return  Guid.Parse((string)responseMessage.Details);
            }

            /**********************************************************************************/

            private bool ProcessActivityResponse(ActivityResponseDTO activityResponse, OperationalStateCM.ActivityExecutionPhase activityExecutionPhase, OperationalStateCM.StackFrame topFrame)
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

                PlanNodeDO currentNode;
                PlanNodeDO targetNode;
                Guid id;

                switch (opCode)
                {
                    case ActivityResponse.Error:
                        var currentActivity = _uow.PlanRepository.GetById<ActivityDO>(topFrame.NodeId);
                        ErrorDTO error = activityResponse.TryParseErrorDTO(out error) ? error : null;
                        ActivityErrorCode errorCode;

                        if (Enum.TryParse(error?.ErrorCode, out errorCode) && errorCode == ActivityErrorCode.AUTH_TOKEN_NOT_PROVIDED_OR_INVALID)
                        {
                            throw new InvalidTokenRuntimeException(Mapper.Map<ActivityDO, ActivityDTO>(currentActivity), 
                                Mapper.Map<ContainerDO, ContainerDTO>(_container), 
                                error?.Message ?? string.Empty);
                        }
                        
                        throw new ErrorResponseException(Mapper.Map<ContainerDO, ContainerDTO>(_container), error?.Message);
                       
                    case ActivityResponse.ExecuteClientActivity:
                        break;

                    case ActivityResponse.ShowDocumentation:
                        break;

                    case ActivityResponse.LaunchAdditionalPlan:
                        LoadAndRunPlan(ExtractGuidParameter(activityResponse));
                        break;

                    case ActivityResponse.RequestTerminate:
                        _callStack.Clear();
                        EventManager.ProcessingTerminatedPerActivityResponse(_container, ActivityResponse.RequestTerminate);
                        return false;

                    case ActivityResponse.RequestSuspend:
                        _container.State = State.Suspended;
                        
                        if (activityExecutionPhase == OperationalStateCM.ActivityExecutionPhase.ProcessingChildren)
                        {
                            _callStack.PushFrame(topFrame);
                        }
                        else
                        {
                            // reset state of currently executed activity
                            topFrame.CurrentActivityExecutionPhase = OperationalStateCM.ActivityExecutionPhase.WasNotExecuted;
                        }

                        return false;

                    case ActivityResponse.SkipChildren:
                        if (activityExecutionPhase == OperationalStateCM.ActivityExecutionPhase.WasNotExecuted)
                        {
                            _callStack.RemoveTopFrame();
                        }
                        break;
                 
                    case ActivityResponse.JumpToSubplan:
                        id = ExtractGuidParameter(activityResponse);
                        targetNode = _uow.PlanRepository.GetById<PlanNodeDO>(id);

                        if (targetNode == null)
                        {
                            throw new InvalidOperationException($"Unable to find node {id}");
                        }


                        // @alexavrutin here: commented this block since this check broke Test and Branch in Kiosk mode 
                        // when a new plan is being created. 
                        // currentNode = _uow.PlanRepository.GetById<PlanNodeDO>(topFrame.NodeId);
                        //if (currentNode.RootPlanNodeId != targetNode.RootPlanNodeId)
                        //{
                        //    throw new InvalidOperationException("Can't jump to the subplan from different plan. Instead, use Jump to Plan.");
                        //}

                        _callStack.Clear();
                        AddNodeForExecution(id);
                        break;

                    case ActivityResponse.JumpToActivity:
                        id = ExtractGuidParameter(activityResponse);
                        targetNode = _uow.PlanRepository.GetById<PlanNodeDO>(id);

                        if (targetNode == null)
                        {
                            throw new InvalidOperationException($"Unable to find node {id}");
                        }

                        currentNode = _uow.PlanRepository.GetById<PlanNodeDO>(topFrame.NodeId);

                        if (currentNode.RootPlanNodeId != targetNode.RootPlanNodeId)
                        {
                            throw new InvalidOperationException("Can't jump to the activity from different plan. Instead, use Jump to Plan.");
                        }

                        if (targetNode.ParentPlanNodeId == null && currentNode.ParentPlanNodeId == null && currentNode.Id != targetNode.Id)
                        {
                            throw new InvalidOperationException("Can't jump from the activities that has no parent to anywhere except the activity itself.");
                        }

                        if (targetNode.ParentPlanNodeId != currentNode.ParentPlanNodeId)
                        {
                            throw new InvalidOperationException("Can't jump to activity that has parent different from activity we are jumping from.");
                        }
                        
                        // we are jumping after activity's Run
                        if (activityExecutionPhase == OperationalStateCM.ActivityExecutionPhase.WasNotExecuted)
                        {
                            // remove the frame representing the current activity from stack. 
                            _callStack.RemoveTopFrame();
                        }

                        if (id == topFrame.NodeId)
                        {
                            // we want to pass current local data (from the topFrame) to the next activity we are calling.
                            _operationalState.BypassData = topFrame.LocalData;
                        }

                        // this is root node. Just push new frame
                        if (_callStack.Count == 0 || currentNode.ParentPlanNode == null)
                        {
                            AddNodeForExecution(id);
                        }
                        else
                        {
                            // find activity that is preceeding the one we are jumping to.
                            // so the next iteration of run cycle will exectute the activity we are jumping to
                            var prevToJump = currentNode.ParentPlanNode.ChildNodes.OrderByDescending(x => x.Ordering).FirstOrDefault(x => x.Ordering < targetNode.Ordering && x.Runnable);

                            _callStack.TopFrame.CurrentChildId = prevToJump?.Id;
                        }

                        break;

                    case ActivityResponse.CallAndReturn:
                        id = ExtractGuidParameter(activityResponse);

                        targetNode = _uow.PlanRepository.GetById<PlanNodeDO>(id);

                        if (targetNode == null)
                        {
                            throw new InvalidOperationException($"Unable to find node {id}");
                        }

                        currentNode = _uow.PlanRepository.GetById<PlanNodeDO>(topFrame.NodeId);

                        if (currentNode.RootPlanNodeId != targetNode.RootPlanNodeId)
                        {
                            throw new InvalidOperationException("Can't call the activity from different plan. Instead, use Jump to Plan.");
                        }

                        AddNodeForExecution(id);
                        break;

                    case ActivityResponse.Break:
                        if (activityExecutionPhase == OperationalStateCM.ActivityExecutionPhase.WasNotExecuted)
                        {
                            // we wan't to have an exception in case of the corrupted stack, so we don't merge this with check below
                            _callStack.RemoveTopFrame();
                        }

                        if (_callStack.Count > 0)
                        {
                            _callStack.RemoveTopFrame();
                        }
                        break;
                }

                return true;
            }

            /**********************************************************************************/

            private void LoadAndRunPlan(Guid planId)
            {
                var plan = ObjectFactory.GetInstance<IPlan>();

                var planDO = _uow.PlanRepository.GetById<PlanDO>(planId);

                if (planDO == null)
                {
                    throw  new InvalidOperationException($"Plan {planId} was not found");
                }

                var crateStorage = _crate.GetStorage(_container.CrateStorage);
                var operationStateCrate = crateStorage.CrateContentsOfType<OperationalStateCM>().Single();

                operationStateCrate.CurrentActivityResponse = ActivityResponseDTO.Create(ActivityResponse.Null);

                operationStateCrate.History.Add(new OperationalStateCM.HistoryElement
                {
                    Description = "Launch Triggered by Container ID " + _container.Id
                });
                
                crateStorage.Remove<OperationalStateCM>();

                var payloadCrates = crateStorage.AsEnumerable().ToArray();

                plan.Enqueue(planDO.Id, payloadCrates);
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
            // this method is for updating Container's payload with the payload that activity returns.
            // We store all information related to container exectution inside the OperationalStateCM crate.
            // Some of this information is internal like the call stack. Call stack is the replacement of the previously used Conatiner.CurrentPlanNodeId.
            // Ideally call stack and the related stuff should not be inside OperationalStateCM. It should be placed inside the container, not the payload.
            // But due to implementation details of activities like Loop, or TestAndBrach we have to store entire call stack inside the payload.
            // We can't allow activity to change frames sequence inside the call stack. 
            // Allowing this is just like allowing one process to write into the memory of another without restrictions.
            // But once again, implementation details of the activities mentioned about requires us to grand some limited access to the stack frames: we allow activity to store some
            // custom data "binded" to the its own stack frame. But we still don't allow activity to rearrage stack frames, otherwise Hub will lose control over the execution.
            // The following methods exists to enfore described constrainsts: it allow to change  custom data, but do not allow rearranging the stack frames.
            private void SyncPayload(ICrateStorage activityPayloadStorage, IUpdatableCrateStorage containerStorage)
            {
                if (activityPayloadStorage == null)
                {
                    return;
                }

                // Update container payload with the payload returned from the activity.
                containerStorage.Replace(activityPayloadStorage);

                // get OperationalStateCM after update.
                _operationalState = GetOperationalState(containerStorage);

                //_operationalState.CallStack.TopFrame.LocalData - is the data activity wants to store within the stack frame
                // store this data in the variable
                var localData = _operationalState.CallStack.TopFrame?.LocalData;
                // and replace call stack wihtin OperationalStateCM with the call stack Hub is using for this exectuion session. 
                _operationalState.CallStack = _callStack;

                // We are 100% call stack is correct and was not corrupted by the activity. 
                // But now we lost the data that activity asked us to persist within the stack frame.
                // Restore this data.
                if (_callStack.Count > 0)
                {
                    _callStack.TopFrame.LocalData = localData;
                }
            }

            /**********************************************************************************/
        }
    }
}
