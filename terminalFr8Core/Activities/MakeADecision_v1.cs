using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Hub.Managers;
using TerminalBase.Infrastructure;

namespace terminalFr8Core.Actions
{
    public class MakeADecision_v1 : TestIncomingData_v1
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
                    Error(crateStorage, "This Action can't run without OperationalStateCM crate", ActivityErrorCode.PAYLOAD_DATA_MISSING);
                    return curPayloadDTO;
                }

                var currentBranch = operationsCrate.CallStack.GetLocalData<OperationalStateCM.BranchStatus>("Branch");
                if (currentBranch == null)
                {
                    currentBranch = CreateBranch();
                }

                currentBranch.Count += 1;

                if (currentBranch.Count >= SlowRunLimit)
                {
                    Error(crateStorage, "This container hit a maximum loop count and was stopped because we're afraid it might be an infinite loop");
                    return curPayloadDTO;
                }

                if (currentBranch.Count >= SmoothRunLimit)
                {
                    //it seems we need to slow down things
                    var diff = DateTime.UtcNow - currentBranch.LastBranchTime;
                    if (diff.TotalSeconds < MinAllowedElapsedTimeInSeconds)
                    {
                        await Task.Delay(10000);
                    }
                }

                currentBranch.LastBranchTime = DateTime.UtcNow;
                
                operationsCrate.CallStack.StoreLocalData("Branch", currentBranch);
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
                            if (!containerTransitionField.TargetNodeId.HasValue)
                            {
                                return Error(curPayloadDTO, "Target Activity for transition is not specified. Please choose it in the Make a Decision activity settings and re-run the Plan.", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
                            }
                            return JumpToActivity(curPayloadDTO, containerTransitionField.TargetNodeId.Value);
                        case ContainerTransitions.LaunchAdditionalPlan:
                            if (!containerTransitionField.TargetNodeId.HasValue)
                            {
                                return Error(curPayloadDTO, "Target Additional Plan for transition is not specified. Please choose it in the Make a Decision activity settings and re-run the Plan.", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
                            }
                            return LaunchAdditionalPlan(curPayloadDTO, containerTransitionField.TargetNodeId.Value);
                        case ContainerTransitions.JumpToSubplan:
                            if (!containerTransitionField.TargetNodeId.HasValue)
                            {
                                return Error(curPayloadDTO, "Target SubPlan for transition is not specified. Please choose it in the Make a Decision activity settings and re-run the Plan.", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
                            }
                            return JumpToSubplan(curPayloadDTO, containerTransitionField.TargetNodeId.Value);
                        case ContainerTransitions.ProceedToNextActivity:
                            return Success(curPayloadDTO);
                        case ContainerTransitions.StopProcessing:
                            return TerminateHubExecution(curPayloadDTO);
                        case ContainerTransitions.SuspendProcessing:
                            throw new NotImplementedException();

                        default:
                            return Error(curPayloadDTO, "Invalid data was selected on MakeADecision_v1#Run", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
                    }
                }
            }

            //none of them matched let's continue normal execution
            return Success(curPayloadDTO);
        }

        private OperationalStateCM.BranchStatus CreateBranch()
        {
            return new OperationalStateCM.BranchStatus
            {
                Count = 0,
                LastBranchTime = DateTime.UtcNow
            };
        }

        private bool CheckConditions(List<FilterConditionDTO> conditions, IQueryable<FieldDTO> fields)
        {
            var filterExpression = ParseCriteriaExpression(conditions, fields);
            foreach (FieldDTO field in fields)
            {
                double result = 0D;
                if (Double.TryParse(field.Value, 
                    System.Globalization.NumberStyles.AllowCurrencySymbol 
                    | System.Globalization.NumberStyles.AllowDecimalPoint 
                    | System.Globalization.NumberStyles.AllowThousands, 
                    System.Globalization.CultureInfo.CurrentCulture,
                    out result))
                {
                    field.Value = result.ToString();
                }
            }

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