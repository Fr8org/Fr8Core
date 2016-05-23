# Terminal Activities
Terminal Activities, or simply Activities, are integral parts of the Fr8 Terminal architecture. Activities define the specific functionality available to a Terminal as well as the UI controls that are available for the activity.

## Security

## Base Implementation

## Enhanced Terminal
Should developers be encouraged to extend the EnhancedTerminalActivity instead of BaseTerminalActivity?

## Fields
* CrateManager
* CurrentFr8UserId
* CurrentFr8UserEmail
* ActivityName
* ActivityTemplateCache
* IHubCommunicator
* ExcludedManifestTypes

## Constructors
* BaseTerminalActivity()

  The no parameter constructor calls the single parameter constructor with the value "unknown". This constructor shold not be used.
* BaseTerminalActivity(string name)

  The single parameter constructor creates a new BaseTerminalActivity instance and sets the ActivityName field to the value of the name parameter. This constructor also initializes the CrateManager and HubCommunicator fields

## Methods
### void SetCurrentUser(string UserId, string UserEmail)

  Sets the CurrentFr8UserId and CurrentFr8UserEmail fields
  * Params
    * string UserId: the id for the current user
    * string UserEmail: the email address for the current user
---
### PayloadDTO SuspendHubExecution(PayloadDTO payload)

  Creates an OperationalStateCM Crate object with a value of ActivityResponse.RequestSuspend
  * Params
    * PayloadDTO payload: a PayloadDTO object that contains (?)
  * returns
    * PayloadDTO object that contains (?)
---
### PayloadDTO TerminateHubExecution(PayloadDTO payloadDTO, string message)

  Creates an OperationalStateCM Crate object with a value of ActivityResponse.RequestTerminate
  * Params
    * PayloadDTO payload: a PayloadDTO object which contains (?)
    * string message: a message that is used for (?)
  * returns
    * PayloadDTO object which contains (?)
---
### PayloadDTO LaunchPlan(PayloadDTO payloadDTO, Guid targetPlanId)

  Creates an OperationalStateCM Crate object with a value of ActivityResponse.LaunchAdditionalPlan
  * Params
    * PayloadDTO payload: a PayloadDTO object which contains (?)
    * Guid targetPlanId: the ID of the plan which should be launched
  * returns
    * PayloadDTO object which contains (?)
---
### PayloadDTO JumpToSubplan(PayloadDTO payload, Guid targetSubplanId)

  Creates an OperationalStateCM Crate object with a value of ActivityResponse.JumpToSubplan
  * Params
    * PayloadDTO payload: a PayloadDTO object which contains (?)
    * Guid targetSubplanId: the ID of the Subplan which should be executed
  * returns
    * PayloadDTO object which contains (?)
---
### PayloadDTO JumpToActivity(PayloadDTO payload, Guid targetActivityId)

  Creates an OperationalStateCM Crate object with a value of ActivityResponse.JumpToActivity
  * Params
    * PayloadDTO payload: A PayloadDTO obejct which contains (?)
    * Guid targetActivityId: the ID of the activity which should be moved to
  * returns
    * PayloadDTO object which contains (?)
---
### PayloadDTO LaunchAdditionalPlan(PayloadDTO payload, Guid targetSubplanId)

  Jumps to an activity that resides in the same subplan as the current activity
  * Params
    * PayloadDTO payload: a PayloadDTO object which contains (?)
    * Guid targetSubplanId: the ID of the subplan which should be launched
  * returns
    * PayloadDTO object which contains (?)
---
### PayloadDTO Success(PayloadDTO payload, string message = "")

  returns a success response in a PayloadDTO
  * Params
    * PayloadDTO payload: the PayloadDTO object which contains (?)
    * string message: defaults to the empty string, used in the CurrentActivityResponse
  * returns
    * PayloadDTO object which contains (?)
---
### void Success(IUpdatableCrateStorage crateStorage, string message = "")

  Creates a success response using IUpdatableCrateStorage
  * Params
    * IUpdatableCrateStorage crateStorage: IUpdatableCrateStroage object which contains (?)
    * string message: defaults to null, sets the message in the CurrentActivityResponse
---
### PayloadDTO ExecuteClientActivity(PayloadDTO payload, string clientActionName)

  Creates a CurrentActivityResponse object which an ActivityResponseDTO of type ExecuteClientActivity
  * Params
    * PayloadDTO payload: the PayloadDTO object which contains (?)
    * string clientActionName: the name fo the activity to execute set as the CurrentClientActivityName of the OperationalState object
  * returns
    * PayloadDTO payload: the PayloadDTO object which contains (?)
---
### void SkipChildren(IUpdatableCrateStorage crateStorage)

  Creates an OperationalState crate whose CurrentActivityResponse type is ActivityResponse.SkipChildren
  * Params
    * IUpdatableCrateStorage crateStorage: an IUpdatableCrateStorage object which contains (?)
---
### PayloadDTO Error(PayloadDTO payload, string errorMessage = null, ActivityErrorCode? errorCode = null, string currentActivity = null, string currentTerminal = null)

  Creates a PayloadDTO with an error message to be sent to the Hub
  * Params
    * PayloadDTO payload: a PayloadDTO object which contains (?)
    * string errorMessage: defaults to null, the message to send to the Hub
    * ActivityErrorCode errorCode: the machine-friendly error code
    * string currentActivity: defaults to null, the name ofo the activity for which the error will be reported
    * string currentTerminal: the name of the terminal where the error occurred
  * returns
    * a PayloadDTO object which contains (?)
---
### PayloadDTO Error(PayloadDTO payload, string errorMessage, ErrorType errorType, ActivityErrorCode? errorCode = null, string currentActivity = null, string currentTerminal = null)

  Creates a PayloadDTO with an error message to be sent to the Hub
  * Params
    * PayloadDTO payload: the PayloadDTO object which contains (?)
    * string errorMessage: the detailed error message to send to the Hub
    * ErrorType errorType: the machine-friendly ErrorType for the error
    * ActivityErrorCode errorCode: defaults to null, the machine-friendly error code associated with the error
    * string currentActivity: defaults to null, the name of the activity which was running when the error occurred
    * string currentTerminal: defaults to null, the name of the terminal which defines the activity which was running when the error occurred
  * returns
    * PayloadDTO object which contains (?)
---
### void Error(IUpdatableCrateStorage crateStorage, string errorMessage = null, ActivityErrorCode? errorCode = null, string currentActivity = null, string currentTerminal = null)

  Creates an error message
  * Params
    * IUpdatableCrateStorage crateStorage: the PayloadDTO which contains (?)
    * string errorMessage: defaults to null, the detailed error message to report
    * ActivityErrorCode errorCode: the machine-friendly error code enum associated with the error
    * string currentActivity: defaults to null, the name of the activity that was running when the error occurred
    * string currentTerminal: defaults to null, the name of the terminal which owns the activity that was running when the error occurred
---
### PayloadDTO NeedsAuthenticationError(PayloadDTO payload)

  Utility method that generates an error when there is no authentication token provided for the activity
  * Params
    * PayloadDTO payload: the PayloadDTO object which contains (?)
  * returns
    * a call to Error with ErrorType.Authentication and ActivityErrorCode.AUTH_TOKEN_NOT_PROVIDED_OR_INVALID
---
### PayloadDTO InvalidTokenError(PayloadDTO payload, string instructionsToUser = null)

  Utility method that generates an error associated with an invalid authentication token
  * Params
    * PayloadDTO payload: the PayloadDTO object which contains (?)
    * string instructionsToUser: defaults to null, a detailed message that will displayed in the UI in order to help the user understand the authentication token failure
  * returns
    * a call to Error with ErrorType ErrorType.Authentication and ActivityErrorCode.AUTH_TOKEN_NOT_PROVIDED_OR_INVALID
---
### virtual async Task<PayloadDTO> ExecuteChildActivities(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)

  Utility method which asynchronously calls Success after extracting the PayloadDTO object from the curActivityDO and containerId parameters
  * Params
    * ActivityDO curActivityDO: the ActivityDO which contains (?)
    * Guid containerId: the ID of the container being run
    * AuthorizationTokenDO authTokenDO: the AuthorizationTokenDO object associated with the request
  * returns
    * the result of Success(PayloadDTO payloadDTO)
---
### bool CheckAuthentication(ActivityDO activity, AuthorizationTokenDO authTokenDO)

  Helper method that checks whether the activity requires authentication and adds a crate with authentication details to the ActivityDO object
  * Params
    * ActivityDO activity: the ActivityDO object whose authentication state should be checked
    * AuthorizationTokenDO authTokenDO: the AuthorizationTokenDO object which contains authentication details
  * returns
    * bool which indicates whether the activity requires authentication and, if so, whether the authentication crate was successfully created
---
### void AddAuthenticationCrate(ActivityDO activityDO, bool revocation)

  Helper method which creates a crate containing authentication details
  * Params
    * ActivityDO activityDO: the ActivityDO object which contains (?)
    * bool revocation: flag indicating whether StandardAuthenticationCM.Revocation should be set
---
### virtual bool NeedsAuthentication(AuthorizationTokenDO authTokenDO)

  Helper method which indicates whether the activity requires authentication
  * Params
    * AuthorizationTokenDO authorizationTokenDO: the AuthorizationTokenDO object used to determine whether the activity requires authentication
  * returns
    * boolean value indicating whether the AuthorizationTokenDO object is null
---
### async Task<PayloadDTO> GetPayload(ActivityDO activityDO, Guid containerId)

  Helper method which uses the HubCommunicator to get a PayloadDTO object
  * Params
    * ActivityDO activityDO: the ActivityDO object which describes the current activity
    * Guid containerId: the ID of the container
  * returns
    * A Task generated by a call to HubCommunicator.GetPayload
---
### async Task<UserDTO> GetCurrentUserData(ActivityDO activityDO, Guid containerId)

  Helper method which uses HubCommunicator to get an UserDTO object
  * Params
    * ActivityDO activityDO: the ActivityDO object which describes the current activity
    * Guid containerId: the ID of the container
  * returns
    * A Task generated by a call to HubCommunicator.GetCurrentUser
---
### async Task<PlanDTO> GetPlansByActivity(string activityId)

  Helper method which users HubCommunicator to get a PlanDTO object
  * Params
    * string activityId: the ID of the activity for which the PlanDTO should be retrieved
  * returns
    * A Task generated by a call to HubCommunicator.GetPlansByActivity
---
### async Task<PlanDTO> UpdatePlan(PlanEmptyDTO plan)

  Helper method which users HubCommunicator to update a PlanDTO object
  * Params
    * PlanEmptyDTO plan: the PlanEmptyDTO object which contains (?)
  * returns
    * A Task generated by a call to HubCommunicator.UpcdatePlan
---
### virtual Task ValidateActivity(ActivityDO activityDo, ICrateStorage currActivityCrateStorage, ValidationManager validationManager)

  Method that should be implemented by subclasses of BaseTerminalActivity. The implementation should return a Task which meets the validation requiresments of the terminal implementation
  * Params
    * ActivityDO activityDo: the ActivityDO object which defines the activity to be validated
    * ICrateStorage currActivityCrateStorage: the ICrateStorage object for the current activity
    * ValidationManager validationManager: the implementation of the ValidationManager which should be used to validate the activity
  * returns
    * A Task which indicates whether the activity configuration is valid
---
### async Task<ActivityDO> ProcessConfigurationRequest(ActivityDO curActivityDO, ConfigurationEvaluator configurationEvaluationResult, AuthorizationTokenDO authToken)

  Processes a configuration request for the activity
  * Params
    * ActivityDO curActivityDO: the ActivityDO object which describes the current Activity
    * ConfigurationEvaluator configurationEvaluationResult: the ConfigurationEvaluator implementation which is used to evaluate the activity configuration
    * AuthorizationTokenDO authToken: The AuthorizationTokenDTO associated with the activity, used if the activity requires authentication
  * returns
    * A Task for an ActivityDO object for the configured Activity
---
### virtual async Task<ActivityDO> Configure(ActivityDO activityDO, AuthorizationTokenDO authTokenDO)

  Public helper method which calls the protected ProcessConfigurationRequest method
  * Params
    * ActivityDO activityDO: the ActivityDO object which describes the current activity
    * AuthorizationTokenDO authTokenDO: the AuthorizationTokenDO object which provides authorization details for the activity
  * returns
    * A Task object generated by a call to ProcessConfigurationRequest
---
### virtual ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)

  This method "evaluates" as to what configuration should be called. Every terminal action will have its own decision making; hence this method must be implemented in the relevant child class.
  * Params
    * ActivityDO curActivityDO: the ActivityDO object that describes the activity
  * returns
    * A ConfigurationRequestType object generated by the ConfigurationEvaluator implementation
---
### virtual async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)

  Virtual method which is used to generate an ActivityDO object which contains configuration information for an activity. Some activities require multiple configuration requests, and the initial configuration request sets the baseline configuration for the activity
  * Params
    * ActivityDO curActivityDO: the ActivityDO which describes the activity
    * AuthorizationTokenDO authTokenDO: the AuthorizationTokenDO object for the activity
  * returns
    * a Task for an ActivityDO object containing the details of the baseline configuration for the activity
---
### virtual async Task<ActivityDO> Activate(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)

Virtual method which signals the activity that it should be moved to an activated state
* Params
  * ActivityDO curActivityDO: the ActivityDO which describes the current activity
  * AuthorizationTokenDO authTokenDO: the AuthorizationTokenDO for the activity
* returns
  * A Task for an ActivityDO object which has been activated
---
### virtual async Task<ActivityDO> Deactivate(ActivityDO curActivityDO)

Virtual method which deactivates an activity
* Params
  * ActivityDO curActivityDO: the ActivityDO which describes the activity
* returns
  * A Task for the ActivityDO which has been deactivated
---
### virtual async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)

Virtual method used to configure the activity after the baseline activity has been completed
* Params
  * ActivityDO activityDO: the ActivityDO which describes the activity to configure
  * AuthorizationTokenDO authTokenDO: the AuthorizationTokenDO used to authorize the activity
* returns
  * A Task for an ActivityDO which has been configured