using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Data.Helpers;
using Fr8.TerminalBase.Infrastructure;

namespace terminalFr8Core.Activities
{
    public class Make_A_Decision_v1 : Test_Incoming_Data_v1
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("f52a0f0f-571c-4530-a49f-c2ff2e18eafd"),
            Name = "Make_A_Decision",
            Label = "Make a Decision",
            Version = "1",
            NeedsAuthentication = false,
            MinPaneWidth = 550,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Process,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

#if DEBUG
        private const int SmoothRunLimit = 5;
        private const int SlowRunLimit = 10;
#else
        private const int SmoothRunLimit = 100;
        private const int SlowRunLimit = 250;
#endif

        private const int MinAllowedElapsedTimeInSeconds = 12;


        public Make_A_Decision_v1(ICrateManager crateManager)
            : base(crateManager)
        {
        }

        public override async Task RunTests()
        {
            //let's check current branch status
            var currentBranch = OperationalState.CallStack.GetLocalData<OperationalStateCM.BranchStatus>("Branch") ??
                                CreateBranch();
            currentBranch.Count += 1;
            if (currentBranch.Count >= SlowRunLimit)
            {
                RaiseError("This container hit a maximum loop count and was stopped because we're afraid it might be an infinite loop");
                return;
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
            OperationalState.CallStack.StoreLocalData("Branch", currentBranch);
            var containerTransition = (ContainerTransition)ConfigurationControls.Controls.Single();

            foreach (var containerTransitionField in containerTransition.Transitions)
            {
                if (CheckConditions(containerTransitionField.Conditions, containerTransition.ResolvedUpstreamFields.AsQueryable()))
                {
                    //let's return whatever this one says
                    switch (containerTransitionField.Transition)
                    {
                        case ContainerTransitions.JumpToActivity:
                            if (!containerTransitionField.TargetNodeId.HasValue)
                            {
                                RaiseError("Target Activity for transition is not specified. Please choose it in the Make a Decision activity settings and re-run the Plan.", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
                                return;
                            }
                            RequestJumpToActivity(containerTransitionField.TargetNodeId.Value);
                            return;
                        case ContainerTransitions.LaunchAdditionalPlan:
                            if (!containerTransitionField.TargetNodeId.HasValue)
                            {
                                RaiseError("Target Additional Plan for transition is not specified. Please choose it in the Make a Decision activity settings and re-run the Plan.", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
                                return;
                            }
                            RequestLaunchPlan(containerTransitionField.TargetNodeId.Value);
                            return;
                        case ContainerTransitions.JumpToSubplan:
                            if (!containerTransitionField.TargetNodeId.HasValue)
                            {
                                RaiseError("Target Subplan for transition is not specified. Please choose it in the Make a Decision activity settings and re-run the Plan.", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
                                return;
                            }
                            RequestJumpToSubplan(containerTransitionField.TargetNodeId.Value);
                            return;
                        case ContainerTransitions.ProceedToNextActivity:
                            Success();
                            return;
                        case ContainerTransitions.StopProcessing:
                            RequestPlanExecutionTermination();
                            return;
                        case ContainerTransitions.SuspendProcessing:
                            throw new NotImplementedException();

                        default:
                            RaiseError("Invalid data was selected on Make_A_Decision_v1#Run", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
                            return;
                    }
                }
            }
            //none of them matched let's continue normal execution
            Success();
        }

        protected override Task Validate()
        {
            ValidationManager.ValidateTransitions((ContainerTransition)ConfigurationControls.Controls.Single());
            return Task.FromResult(0);
        }

        private OperationalStateCM.BranchStatus CreateBranch()
        {
            return new OperationalStateCM.BranchStatus
            {
                Count = 0,
                LastBranchTime = DateTime.UtcNow
            };
        }

        private bool CheckConditions(List<FilterConditionDTO> conditions, IQueryable<KeyValueDTO> fields)
        {
            foreach (var field in fields)
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

            var checker = false;
            foreach (var condition in conditions)
            {
                var expression = ParseCriteriaExpression(condition, fields);
                var results = fields.Provider.CreateQuery<KeyValueDTO>(expression);

                if (results.Any())
                {
                    checker = true;
                }
                else
                {
                    //if there is false condition, stop evaluating
                    checker = false;
                    break;
                }
            }
            return checker;
        }

        protected override void CreateControls()
        {
            var transition = new ContainerTransition
            {
                Label = "Please enter transition",
                Name = "transition",
                Source = new FieldSourceDTO()
                {
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields,
                    RequestUpstream = true
                }
            };

            AddControls(transition);
        }
    }
}