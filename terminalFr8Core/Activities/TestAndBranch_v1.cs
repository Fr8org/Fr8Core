using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using AutoMapper;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using TerminalBase;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities;

namespace terminalFr8Core.Actions
{
    public class TestAndBranch_v1 : TestIncomingData_v1
    {
#if DEBUG
        private const int SmoothRunLimit = 5;
        private const int SlowRunLimit = 10;
#else
        private const int SmoothRunLimit = 100;
        private const int SlowRunLimit = 250;
#endif

        private const int MinAllowedElapsedTimeInSeconds = 12;

        public override async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetPayload(curActivityDO, containerId);
            

            //let's check current branch status
            using (var crateStorage = CrateManager.GetUpdatableStorage(curPayloadDTO))
            {
                var operationsCrate = crateStorage.CrateContentsOfType<OperationalStateCM>().FirstOrDefault();
                if (operationsCrate == null)
                {
                    return Error(curPayloadDTO, "This Action can't run without OperationalStateCM crate", ActivityErrorCode.PAYLOAD_DATA_MISSING);
                }

                var currentBranch = GetCurrentBranch(operationsCrate, curActivityDO.GetLoopId());
                if (currentBranch == null)
                {
                    currentBranch = CreateBranch(curActivityDO.GetLoopId());
                    operationsCrate.Branches.Add(currentBranch);
                }

                currentBranch.Count += 1;
                
                if (currentBranch.Count >= SlowRunLimit)
                {
                    return Error(curPayloadDTO, "This container hit a maximum loop count and was stopped because we're afraid it might be an infinite loop");
                }
                else if (currentBranch.Count >= SmoothRunLimit)
                {
                    //it seems we need to slow down things
                    var diff = DateTime.UtcNow - currentBranch.LastBranchTime;
                    if (diff.TotalSeconds < MinAllowedElapsedTimeInSeconds)
                    {
                        await Task.Delay(10000);
                    }
                }

                currentBranch.LastBranchTime = DateTime.UtcNow;
            }


            var payloadFields = GetAllPayloadFields(curPayloadDTO).Where(f => !string.IsNullOrEmpty(f.Key) && !string.IsNullOrEmpty(f.Value)).AsQueryable();

            var configControls = GetConfigurationControls(curActivityDO);
            var containerTransition = (ContainerTransition)configControls.Controls.Single();

            foreach (var containerTransitionField in containerTransition.Transitions)
            {
                if (CheckConditions(containerTransitionField.Conditions, payloadFields))
                {
                    //let's return whatever this one says
                    switch (containerTransitionField.Transition)
                    {
                            case ContainerTransitions.JumpToActivity:
                            //TODO check if targetNodeId is selected
                            return JumpToActivity(curPayloadDTO, containerTransitionField.TargetNodeId.Value);
                            case ContainerTransitions.JumpToPlan:
                            return LaunchPlan(curPayloadDTO, containerTransitionField.TargetNodeId.Value);
                            case ContainerTransitions.JumpToSubplan:
                            return LaunchAdditionalPlan(curPayloadDTO, containerTransitionField.TargetNodeId.Value);
                            case ContainerTransitions.ProceedToNextActivity:
                            return Success(curPayloadDTO);
                            case ContainerTransitions.StopProcessing:
                            return TerminateHubExecution(curPayloadDTO);
                            case ContainerTransitions.SuspendProcessing:
                            throw new NotImplementedException();

                        default:
                            return Error(curPayloadDTO, "Invalid data was selected on TestAndBranch_v1#Run", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
                    }
                }
            }

            //none of them matched let's continue normal execution
            return Success(curPayloadDTO);
        }

        private OperationalStateCM.BranchStatus GetCurrentBranch(OperationalStateCM operationalState, string currentActivityId)
        {
            return operationalState.Branches.FirstOrDefault(l => l.Id == currentActivityId);
        }

        private OperationalStateCM.BranchStatus CreateBranch(string currentActivityId)
        {
            return new OperationalStateCM.BranchStatus
            {
                Count = 0,
                Id = currentActivityId,
                LastBranchTime = DateTime.UtcNow
            };
        }

        private bool CheckConditions(List<FilterConditionDTO> conditions, IQueryable<FieldDTO> fields)
        {
            var filterExpression = ParseCriteriaExpression(conditions, fields);
            var results = fields.Provider.CreateQuery<FieldDTO>(filterExpression);
            return results.Any();

        }
        
        protected override Crate CreateControlsCrate()
        {
            var transition = new ContainerTransition
            {
                Label = "Please enter transition",
                Name = "transition"
            };


            return PackControlsCrate(transition);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            var controlsMS = CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (controlsMS == null)
            {
                return ConfigurationRequestType.Initial;
            }

            var durationControl = controlsMS.Controls.FirstOrDefault(x => x.Type == ControlTypes.ContainerTransition && x.Name == "transition");

            if (durationControl == null)
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
    }
}