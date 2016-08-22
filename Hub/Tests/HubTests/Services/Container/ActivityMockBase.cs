using System;
using System.Linq;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.DataTransferObjects.Helpers;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;

namespace HubTests.Services.Container
{
    public class ActivityMockBase
    {
        /**********************************************************************************/
        // Declarations
        /**********************************************************************************/

        private readonly ICrateManager _crateManager;

        /**********************************************************************************/

        protected OperationalStateCM OperationalState
        {
            get;
            private set;
        }

        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

        public ActivityMockBase(ICrateManager crateManager)
        {
            _crateManager = crateManager;
        }

        /**********************************************************************************/
        
        public void Run(Guid id, ActivityExecutionMode executionMode, PayloadDTO payloadDto)
        {
            using (var storage = _crateManager.UpdateStorage(() => payloadDto.CrateStorage))
            {
                OperationalState = storage.CrateContentsOfType<OperationalStateCM>().First();
                Run(id, executionMode);
            }
        }

        /**********************************************************************************/

        protected virtual void Run(Guid id, ActivityExecutionMode executionMode)
        {
        }

        /**********************************************************************************/
        /// <summary>
        /// Creates a suspend request for hub execution
        /// </summary>
        protected void RequestHubExecutionSuspension(string message = null)
        {
            SetResponse(ActivityResponse.RequestSuspend, message);
        }

        /**********************************************************************************/
        /// <summary>
        /// Creates a terminate request for hub execution
        /// after that we could stop throwing exceptions on actions
        /// </summary>
        protected void RequestHubExecutionTermination(string message = null)
        {
            SetResponse(ActivityResponse.RequestTerminate, message);
        }

        /**********************************************************************************/
        /// <summary>
        /// returns success to hub
        /// </summary>
        protected void Success(string message = null)
        {
            SetResponse(ActivityResponse.Success, message);
        }

        /**********************************************************************************/

        protected void RequestClientActivityExecution(string clientActionName)
        {
            SetResponse(ActivityResponse.ExecuteClientActivity);
            OperationalState.CurrentActivityResponse.Body = clientActionName;
        }

        /**********************************************************************************/
        /// <summary>
        /// skips children of this action
        /// </summary>
        protected void RequestSkipChildren()
        {
            SetResponse(ActivityResponse.SkipChildren);
        }

        /**********************************************************************************/
        /// <summary>
        /// Jumps to an activity that resides in same subplan as current activity
        /// </summary>
        /// <returns></returns>
        protected void RequestJumpToActivity(Guid targetActivityId)
        {
            SetResponse(ActivityResponse.JumpToActivity);
            OperationalState.CurrentActivityResponse.AddResponseMessageDTO(new ResponseMessageDTO() { Details = targetActivityId });
        }

        /**********************************************************************************/
        /// <summary>
        /// Call an activity or subplan  and return to the current activity
        /// </summary>
        /// <returns></returns>
        protected void RequestCall(Guid targetNodeId)
        {
            SetResponse(ActivityResponse.CallAndReturn);
            OperationalState.CurrentActivityResponse.AddResponseMessageDTO(new ResponseMessageDTO() { Details = targetNodeId });
        }

        /**********************************************************************************/
        /// <summary>
        /// Jumps to an activity that resides in same subplan as current activity
        /// </summary>
        /// <returns></returns>
        protected void RequestJumpToSubplan(Guid targetSubplanId)
        {
            SetResponse(ActivityResponse.JumpToSubplan);
            OperationalState.CurrentActivityResponse.AddResponseMessageDTO(new ResponseMessageDTO() { Details = targetSubplanId });
        }

        /**********************************************************************************/

        protected void SetResponse(ActivityResponse response, string message = null, object details = null)
        {
            OperationalState.CurrentActivityResponse = ActivityResponseDTO.Create(response);

            if (!string.IsNullOrWhiteSpace(message) || details != null)
            {
                OperationalState.CurrentActivityResponse.AddResponseMessageDTO(new ResponseMessageDTO() { Message = message, Details = details });
            }
        }

        /**********************************************************************************/
        /// <summary>
        /// returns error to hub
        /// </summary>
        protected void Error(string errorMessage = null, ActivityErrorCode? errorCode = null)
        {
            SetResponse(ActivityResponse.Error);
            OperationalState.CurrentActivityResponse.AddErrorDTO(ErrorDTO.Create(errorMessage, ErrorType.Generic, errorCode.ToString(), null, null, null));
        }

        /**********************************************************************************/
    }
}