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
        public override async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetPayload(curActivityDO, containerId);
            var payloadFields = GetAllPayloadFields(curPayloadDTO).AsQueryable();

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
                            throw new NotImplementedException();
                            case ContainerTransitions.JumpToSubplan:
                            return JumpToSubplan(curPayloadDTO, containerTransitionField.TargetNodeId.Value);
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