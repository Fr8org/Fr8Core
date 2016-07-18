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
**BaseTerminalActivity()**

 The no parameter constructor calls the single parameter constructor with the value "unknown". This constructor shold not be used.

**BaseTerminalActivity(string name)**

 The single parameter constructor creates a new BaseTerminalActivity instance and sets the ActivityName field to the value of the name parameter. This constructor also initializes the CrateManager and HubCommunicator fields

## Methods
**void SetCurrentUser(string UserId, string UserEmail)**

  Sets the CurrentFr8UserId and CurrentFr8UserEmail fields
* Params
  * string UserId: the id for the current user
  * string UserEmail: the email address for the current user

---
**[PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) SuspendHubExecution([PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) payload)**

  Creates an OperationalStateCM Crate object with a value of ActivityResponse.RequestSuspend
* Params
  * [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) payload: a [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) object that contains (?)
* returns
  * [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) object that contains (?)

---
**[PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) TerminateHubExecution([PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md), string message)**

  Creates an OperationalStateCM Crate object with a value of ActivityResponse.RequestTerminate
* Params
  * [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) payload: a [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) object which contains (?)
  * string message: a message that is used for (?)
* returns
  * [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) object which contains (?)

---
**[PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) LaunchPlan([PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md), Guid targetPlanId)**

  Creates an OperationalStateCM Crate object with a value of ActivityResponse.LaunchAdditionalPlan
* Params
  * [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) payload: a [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) object which contains (?)
  * Guid targetPlanId: the ID of the plan which should be launched
* returns
  * [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) object which contains (?)

---
**[PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) JumpToSubplan([PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) payload, Guid targetSubplanId)**

  Creates an OperationalStateCM Crate object with a value of ActivityResponse.JumpToSubplan
* Params
  * [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) payload: a [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) object which contains (?)
  * Guid targetSubplanId: the ID of the Subplan which should be executed
* returns
  * [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) object which contains (?)

---
**[PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) JumpToActivity([PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) payload, Guid targetActivityId)**

  Creates an OperationalStateCM Crate object with a value of ActivityResponse.JumpToActivity
* Params
  * [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) payload: A [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) obejct which contains (?)
  * Guid targetActivityId: the ID of the activity which should be moved to
* returns
  * [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) object which contains (?)

---
**[PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) LaunchAdditionalPlan([PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) payload, Guid targetSubplanId)**

  Jumps to an activity that resides in the same subplan as the current activity
* Params
  * [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) payload: a [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) object which contains (?)
  * Guid targetSubplanId: the ID of the subplan which should be launched
* returns
  * [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) object which contains (?)

---
**[PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) Success([PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) payload, string message = "")**

  returns a success response in a [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md)
* Params
  * [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) payload: the [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) object which contains (?)
  * string message: defaults to the empty string, used in the CurrentActivityResponse
* returns
  * [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) object which contains (?)

---
**void Success(IUpdatableCrateStorage crateStorage, string message = "")**

  Creates a success response using IUpdatableCrateStorage
* Params
  * IUpdatableCrateStorage crateStorage: IUpdatableCrateStroage object which contains (?)
  * string message: defaults to null, sets the message in the CurrentActivityResponse

---
**[PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) ExecuteClientActivity([PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) payload, string clientActionName)**

  Creates a CurrentActivityResponse object which an ActivityResponseDTO of type ExecuteClientActivity
* Params
  * [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) payload: the [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) object which contains (?)
  * string clientActionName: the name fo the activity to execute set as the CurrentClientActivityName of the OperationalState object
* returns
  * [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) payload: the [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) object which contains (?)

---
**void SkipChildren(IUpdatableCrateStorage crateStorage)**

  Creates an OperationalState crate whose CurrentActivityResponse type is ActivityResponse.SkipChildren
* Params
  * IUpdatableCrateStorage crateStorage: an IUpdatableCrateStorage object which contains (?)

---
**[PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) Error([PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) payload, string errorMessage = null, ActivityErrorCode? errorCode = null, string currentActivity = null, string currentTerminal = null)**

  Creates a [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) with an error message to be sent to the Hub
* Params
  * [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) payload: a [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) object which contains (?)
  * string errorMessage: defaults to null, the message to send to the Hub
  * ActivityErrorCode errorCode: the machine-friendly error code
  * string currentActivity: defaults to null, the name ofo the activity for which the error will be reported
  * string currentTerminal: the name of the terminal where the error occurred
* returns
  * a [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) object which contains (?)

---
**[PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) Error([PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) payload, string errorMessage, ErrorType errorType, ActivityErrorCode? errorCode = null, string currentActivity = null, string currentTerminal = null)**

  Creates a [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) with an error message to be sent to the Hub
* Params
  * [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) payload: the [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) object which contains (?)
  * string errorMessage: the detailed error message to send to the Hub
  * ErrorType errorType: the machine-friendly ErrorType for the error
  * ActivityErrorCode errorCode: defaults to null, the machine-friendly error code associated with the error
  * string currentActivity: defaults to null, the name of the activity which was running when the error occurred
  * string currentTerminal: defaults to null, the name of the terminal which defines the activity which was running when the error occurred
* returns
   * [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) object which contains (?)

---
**void Error(IUpdatableCrateStorage crateStorage, string errorMessage = null, ActivityErrorCode? errorCode = null, string currentActivity = null, string currentTerminal = null)**

  Creates an error message
* Params
  * IUpdatableCrateStorage crateStorage: the [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) which contains (?)
  * string errorMessage: defaults to null, the detailed error message to report
  * ActivityErrorCode errorCode: the machine-friendly error code enum associated with the error
  * string currentActivity: defaults to null, the name of the activity that was running when the error occurred
  * string currentTerminal: defaults to null, the name of the terminal which owns the activity that was running when the error occurred

---
**[PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) NeedsAuthenticationError([PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) payload)**

  Utility method that generates an error when there is no authentication token provided for the activity
* Params
  * [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) payload: the [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) object which contains (?)
* returns
  * a call to Error with ErrorType.Authentication and ActivityErrorCode.AUTH_TOKEN_NOT_PROVIDED_OR_INVALID

---
**[PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) InvalidTokenError([PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) payload, string instructionsToUser = null)**

  Utility method that generates an error associated with an invalid authentication token
* Params
  * [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) payload: the [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) object which contains (?)
  * string instructionsToUser: defaults to null, a detailed message that will displayed in the UI in order to help the user understand the authentication token failure
* returns
  * a call to Error with ErrorType ErrorType.Authentication and ActivityErrorCode.AUTH_TOKEN_NOT_PROVIDED_OR_INVALID

---
**virtual async Task<[PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md)> ExecuteChildActivities(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)**

  Utility method which asynchronously calls Success after extracting the [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) object from the curActivityDO and containerId parameters
* Params
  * ActivityDO curActivityDO: the ActivityDO which contains (?)
  * Guid containerId: the ID of the container being run
  * AuthorizationTokenDO authTokenDO: the AuthorizationTokenDO object associated with the request
* returns
  * the result of Success([PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md))

---
**bool CheckAuthentication(ActivityDO activity, AuthorizationTokenDO authTokenDO)**

  Helper method that checks whether the activity requires authentication and adds a crate with authentication details to the ActivityDO object
* Params
  * ActivityDO activity: the ActivityDO object whose authentication state should be checked
  * AuthorizationTokenDO authTokenDO: the AuthorizationTokenDO object which contains authentication details
* returns
  * bool which indicates whether the activity requires authentication and, if so, whether the authentication crate was successfully created

---
**void AddAuthenticationCrate(ActivityDO activityDO, bool revocation)**

  Helper method which creates a crate containing authentication details
* Params
  * ActivityDO activityDO: the ActivityDO object which contains (?)
  * bool revocation: flag indicating whether StandardAuthenticationCM.Revocation should be set

---
**virtual bool NeedsAuthentication(AuthorizationTokenDO authTokenDO)**

  Helper method which indicates whether the activity requires authentication
* Params
  * AuthorizationTokenDO authorizationTokenDO: the AuthorizationTokenDO object used to determine whether the activity requires authentication
* returns
  * boolean value indicating whether the AuthorizationTokenDO object is null

---
**async Task<[PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md)> GetPayload(ActivityDO activityDO, Guid containerId)**

  Helper method which uses the HubCommunicator to get a [PayloadDTO](/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md) object
* Params
  * ActivityDO activityDO: the ActivityDO object which describes the current activity
  * Guid containerId: the ID of the container
* returns
  * A Task generated by a call to HubCommunicator.GetPayload

---
**async Task<UserDTO> GetCurrentUserData(ActivityDO activityDO, Guid containerId)**

  Helper method which uses HubCommunicator to get an UserDTO object
* Params
  * ActivityDO activityDO: the ActivityDO object which describes the current activity
  * Guid containerId: the ID of the container
* returns
  * A Task generated by a call to HubCommunicator.GetCurrentUser

---
**async Task<PlanDTO> GetPlansByActivity(string activityId)**

  Helper method which users HubCommunicator to get a PlanDTO object
* Params
  * string activityId: the ID of the activity for which the PlanDTO should be retrieved
* returns
  * A Task generated by a call to HubCommunicator.GetPlansByActivity

---
**async Task<PlanDTO> UpdatePlan(PlanEmptyDTO plan)**

  Helper method which users HubCommunicator to update a PlanDTO object
* Params
  * PlanEmptyDTO plan: the PlanEmptyDTO object which contains (?)
* returns
  * A Task generated by a call to HubCommunicator.UpcdatePlan

---
**virtual Task ValidateActivity(ActivityDO activityDo, ICrateStorage currActivityCrateStorage, ValidationManager validationManager)**

  Method that should be implemented by subclasses of BaseTerminalActivity. The implementation should return a Task which meets the validation requiresments of the terminal implementation
* Params
  * ActivityDO activityDo: the ActivityDO object which defines the activity to be validated
  * ICrateStorage currActivityCrateStorage: the ICrateStorage object for the current activity
  * ValidationManager validationManager: the implementation of the ValidationManager which should be used to validate the activity
* returns
  * A Task which indicates whether the activity configuration is valid

---
**async Task<ActivityDO> ProcessConfigurationRequest(ActivityDO curActivityDO, ConfigurationEvaluator configurationEvaluationResult, AuthorizationTokenDO authToken)**

  Processes a configuration request for the activity
* Params
  * ActivityDO curActivityDO: the ActivityDO object which describes the current Activity
  * ConfigurationEvaluator configurationEvaluationResult: the ConfigurationEvaluator implementation which is used to evaluate the activity configuration
  * AuthorizationTokenDO authToken: The AuthorizationTokenDTO associated with the activity, used if the activity requires authentication
* returns
  * A Task for an ActivityDO object for the configured Activity

---
**virtual async Task<ActivityDO> Configure(ActivityDO activityDO, AuthorizationTokenDO authTokenDO)**

  Public helper method which calls the protected ProcessConfigurationRequest method
* Params
  * ActivityDO activityDO: the ActivityDO object which describes the current activity
  * AuthorizationTokenDO authTokenDO: the AuthorizationTokenDO object which provides authorization details for the activity
* returns
  * A Task object generated by a call to ProcessConfigurationRequest

---
**virtual ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)**

  This method "evaluates" as to what configuration should be called. Every terminal action will have its own decision making; hence this method must be implemented in the relevant child class.
* Params
  * ActivityDO curActivityDO: the ActivityDO object that describes the activity
* returns
  * A ConfigurationRequestType object generated by the ConfigurationEvaluator implementation

---
**virtual async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)**

 Virtual method which is used to generate an ActivityDO object which contains configuration information for an activity. Some activities require multiple configuration requests, and the initial configuration request sets the baseline configuration for the activity
* Params
  * ActivityDO curActivityDO: the ActivityDO which describes the activity
  * AuthorizationTokenDO authTokenDO: the AuthorizationTokenDO object for the activity
* returns
  * a Task for an ActivityDO object containing the details of the baseline configuration for the activity

---
**virtual async Task<ActivityDO> Activate(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)**

Virtual method which signals the activity that it should be moved to an activated state
* Params
  * ActivityDO curActivityDO: the ActivityDO which describes the current activity
  * AuthorizationTokenDO authTokenDO: the AuthorizationTokenDO for the activity
* returns
  * A Task for an ActivityDO object which has been activated

---
**virtual async Task<ActivityDO> Deactivate(ActivityDO curActivityDO)**

Virtual method which deactivates an activity
* Params
  * ActivityDO curActivityDO: the ActivityDO which describes the activity
* returns
  * A Task for the ActivityDO which has been deactivated

---
**virtual async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)**

Virtual method used to configure the activity after the baseline activity has been completed
* Params
  * ActivityDO activityDO: the ActivityDO which describes the activity to configure
  * AuthorizationTokenDO authTokenDO: the AuthorizationTokenDO used to authorize the activity
* returns
  * A Task for an ActivityDO which has been configured

---
**void UpdateDesignTimeCrateValue(ICrateStorage storage, string label, params FieldDTO[] fields)**

Method to update the design time crate for Activity configurations
* Params
  * ICrateStorage storage: the ICrateStorage object to update
  * string label: the name of the fields to use when querying and inserting
  * FieldDTO[] fields: a collection of FieldDTO objects to store in the crate storage

---
**async Task<ActivityTemplateDTO> GetActivityTemplate(Guid activityTemplateId)**

Retrieve an activity template
* Params
  * Guid activityTemplateId: the ID of the template to retrieve
* returns
  * A Task for an ActivityTemplateDTO object representing the ActivityTemplate entity whose ID is the value of the activityTemplateId parameter

---
**async Task<ActivityTemplateDTO> GetActivityTemplate(string terminalName, string activityTemplateName, string activityTemplateVersion = "1", string terminalVersion = "1")**

Retrieve an ActivityTemplate object by name, terminal name, template version, and terminal version
* Params
  * string terminalName: the name of the terminal to which the ActivityTemplate belongs
  * string activityTemplateName: the name of the ActivityTemplate object
  * string activityTemplateVersion: defaults to 1, the value of the activityTemplateVersion to use when querying for the ActivityTemplate object
  * string terminalVersion: defaults to 1, the value of the terminalVersion to use when querying for the ActivityTemplate object

---
**async Task<ActivityDO> AddAndConfigureChildActivity(Guid parentActivityId, ActivityTemplateDTO activityTemplate, string name = null, string label = null, int? order = null)**

Adds and configures a child activity to another activity
* Params
  * Guid parentActivityId: the ID of the Activity to which the child Activity will be added
  * ActivityTemplateDTO activityTemplate: the ActivityTemplateDTO that represents the child activity
  * string name: defaults to null, the name of the child Activity
  * string label: defaults to null, the human-friendly label for the Activity
  * int order: the order in which the activity is placed among sibling child Activity objects
* returns
  * A Task object for an ActivityDO which represents the newly created Activity

---
**async Task<ActivityDO> AddAndConfigureChildActivity(ActivityDO parent, ActivityTemplateDTO activityTemplate, string name = null, string label = null, int? order = null)**

Higher order function to create a new Activity as a child of another Activity
* Params
  * ActivityDO parent: the ActivityDO object that represents the parent Activity
  * ActivityTemplateDTO activityTemplate: the ActivityTemplateDTO for the new child Activity
  * string name: the name of the child Activity, defaults to null
  * string label: the human-friendly label of the child Activity, defaults to null
  * int order: the order in which the activity is placed among sibling child Activity objects, defaults to null
* returns
  * A Task for the ActivityDO which represents the newly created child Activity

---
**async Task<ActivityDO> ConfigureChildActivity(ActivityDO parent, ActivityDO child)**

Helper method to create a new child Activity
* Params
  * ActivityDO parent: the ActivityDO object representing the parent Activity
  * ActivityDO child: the ActivityDO object representing the child Activity
* returns
  * A Task for the ActivityDO which represents the configured child Activity

---
**async Task<PlanFullDTO> UpdatePlanName(Guid activityId, string OriginalPlanName, string NewPlanName)**

Utility method to update the name of a plan
* Params
  * Guid activityId: the ID of the activity
  * string OriginalPlanName: the current name of the plan
  * string NewPlanName: the new name for the plan
* returns
  * A Task for the PlanFullDTO object

---
**async Task<PlanFullDTO> UpdatePlanCategory(Guid activityId, string category)**

Helper method to update the plan category
* Params
  * Guid activityId: the ID of the Activity to update
  * string category: the name of the category to update
* returns
  * A Task for the PlanFullDTO object

---
**ActivityResponseDTO GenerateDocumentationResponse(string documentation)**

Helper method to generate a documentation response
* Params
  * string documentation: the value to use when setting the ActivityResponseDTO.Body field
* returns
  * A ActivityResponseDTO object whose type is ActivityResponse.ShowDocumentation and whose Body is set to the value of the documentation parameter

---
**ActivityResponseDTO GenerateErrorResponse(string errorMessage)**

Helper method to generate an ActivityResponseDTO whose Type is ActivityResponse.ShowDocumentation (is this right? should it be ActivityResponse.ERROR)
* Params
  * string errorMessage: the value to use when setting the ActivityResponseDTO.Body field
* response
  * a newly created ActivityResponseDTO object whose Body field is set to the value of the errorMessage parameter

---
**SolutionPageDTO GetDefaultDocumentation(string solutionName, double solutionVersion, string terminalName, string body)**

Helper method which creates a new SolutionPageDTO object
* Params
  * string solutionName: the value to use when setting the SolutionPageDTO.Name field
  * double solutionVersion: the value to use when setting the SolutionPageDTO.Version field
  * string terminalName: the value to use when setting the SolutionPageDTO.Terminal field
  * string body: the value to use when setting the SolutionPageDTO.Body field
* returns
  * a newly created SolutionPageDTO object

---
**string ExtractPayloadFieldValue(ICrateStorage crateStorage, string fieldKey, ActivityDO curActivity)**

Extracts the value of the field with the specified key from the ActivityDO object
* Params
  * ICrateStorage crateStorage: the ICrateStorage object where the field exists
  * string fieldKey: the key associated with the field to extract
  * ActivityDO curActivity: the ActivityDO to use when extracting the user ID
* returns
  * string the value of the payload field

---
**virtual async Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(ActivityDO activityDO, CrateDirection direction)**

Gets upstream or downstream crates from an ActivityDO
* Params
  * ActivityDO activityDO: the ActivityDO which contains the crates
  * CrateDirection direction: a CrateDirection enum value which specifies the direction to search when extracting the crates. One of: Upstream, Downstream, Both
* returns
  * A Task object for a list of Crate Manifest objects

---
**Task<IncomingCratesDTO> GetAvailableData(ActivityDO activity, CrateDirection direction = CrateDirection.Upstream, AvailabilityType availability = AvailabilityType.RunTime)**

Helper method which calls HubCommunicator.GetAvailableData
* Params
  * ActivityDO activity: the ActivityDO object to use when retrieving the data
  * CrateDirection direction: the direction to search when extracting data. Defaults to CrateDirection.Upstream.
  * AvailabilityType availability: the AvailabilityType enum value to use when extracting data. Defaults to AvailabilityType.RunTime
* returns
  * A Task object for an IncomingCratesDTO object

---
**virtual async Task<List<Crate>> GetCratesByDirection(ActivityDO activityDO, CrateDirection direction)**

Helper method which calls HubCommunicator.GetCratesByDirection
* Params
  * ActivityDO activityDO: the ActivityDO object to use when retrieving the crates
  * CrateDirection direction: the direction to search when retrieving the crates. One of Upstream, Downstream, or Both
* returns
  * A Task object which contains a list of Crate objects

---
**virtual async Task<FieldDescriptionsCM> GetDesignTimeFields(ActivityDO activityDO, CrateDirection direction, AvailabilityType availability = AvailabilityType.NotSet)**

Helper method which calls HubCommunicator.GetDesignTimeFieldsByDirection
* Params
  * ActivityDO activityDO: the ActivityDO object to use when searching for the design time fields
  * CrateDirection direction: the direction to search when getting the design time fields
  * AvailabilityType availability: the AvailabilityType enum to use when querying for the fields. Defaults to AvailabilityType.NotSet

---
**virtual IEnumerable<FieldDTO> GetRequiredFields(ActivityDO curActivityDO, string crateLabel)**

Helper method which extracts fields from the ActivityDO whose IsRequired property is true
* Params
  * ActivityDO curActivityDO: the ActivityDO object whose required fields will be extracted
  * string crateLabel: the label to use when querying the fields in the ActivityDO object
* returns
  * an IEnumerable collection of FieldDTO objects whose IsRequired property is true

---
**virtual async Task<List<CrateManifestType>> BuildUpstreamManifestList(ActivityDO activityDO)**

Extracts all upstream fields from an ActivityDO
* Params
  * ActivityDO activityDO: the dActivityDO object from which the CrateManifestType objects will be extracted
* returns
  * a Task object which contains a List of CrateManifestType objects

---
**virtual async Task<List<String>> BuildUpstreamCrateLabelList(ActivityDO activityDO)**

Extracts the labels of all upstream crates from an ActivityDO object
* Params
  * ActivityDO activityDO: the ActivityDO object from which the crate labels will be extracted
* returns
  * A Task object for a List of unique strings for the crate labels

---
**virtual async Task<Crate<FieldDescriptionsCM>> GetUpstreamManifestListCrate(ActivityDO activityDO)**

Helper method to generate a new design-time fields crate
* Params
  * ActivityDO activityDO: the ActivityDO object for which the FieldDescriptionsCM object will be created
* returns
  * A Task object for a Crate of FieldDescriptionsCM type

---
**virtual async Task<Crate<FieldDescriptionsCM>> GetUpstreamCrateLabelListCrate(ActivityDO activityDO)**

Generates a Crate of upstream crate labels for an ActivityDO object
* Params
  * ActivityDO activityDO: an ActivityDO object from which the labels will be extracted and added to a Crate object
* returns
  * A Task for a Crate object of FieldDescriptionsCM type

---
**virtual async Task<List<Crate<StandardFileDescriptionCM>>> GetUpstreamFileHandleCrates(ActivityDO activityDO)**

Helper method which calls HubCommunicator.GetCratesByDirection and returns a Crate of StandardFileDescriptionCM type
* Params
  * ActivityDO activityDO: the ActivityDO object from which to extract the Crate objects of StandardFileDescriptionCM
* returns
  * A Task for a List of Crate objects whose type is StandardFileDescriptionCM

---
**async Task<Crate<FieldDescriptionsCM>> MergeUpstreamFields(ActivityDO activityDO, string label)**

Merges the upstream fields in to a Crate of FieldDescriptionCM objects with the label specified by the label parameter
* Params
  * ActivityDO activityDO: the ActivityDO object from which the fields should be extracted
  * string label: the name of the design time fields crate that is returned
* returns
  * A Task object for a Crate with FieldDescriptionsCM type

---
**async Task<Crate> CreateAvailableFieldsCrate(ActivityDO activityDO, string crateLabel = "Upstream Terminal-Provided Fields", AvailabilityType availabilityTypeUpstream = AvailabilityType.RunTime, AvailabilityType availabilityTypeFieldsCrate = AvailabilityType.Configuration)**

Creates a crate with design-time fields
* Params
  * ActivityDO activityDO: the ActivityDO object from which the available fields will be extracted
  * string crateLabel: the name of the crate that will be returned. Defaults to "Upstream Terminal-Provided Fields"
  * AvailabilityType availabilityTypeUpstream: the AvailabilityType enum value of the fields to search for. Defaults to AvailabilityType.RunTime
  * AvailabilityType availabilityTypeFieldsCrate: the AvailabilityType enum value of the crate that is returned
* returns
  * A Task object for a Crate which contains upstream fields

---
**async Task<Crate<FieldDescriptionsCM>> CreateDesignTimeFieldsCrate(ActivityDO curActivityDO, string label)**

Creates a create of design-time field objects from the provided ActivityDO object
* Params
  * ActivityDO curActivityDO: the ActivityDO object from which the design-time fields will be extracted
  * string label: the label for the crate object that will be returned
* returns
  * A Task object for a Crate of type FieldDescriptionCM

---
**async Task<StandardTableDataCM> ExtractDataFromUpstreamCrates(string upstreamCrateChooserName, ActivityDO curActivityDO)**

Extracts upstream table data from the specified ActivityDO
* Params
  * string upstreamCrateChooserName: the name of the control to extract
  * ActivityDO curActivityDO: the ActivityDO object from which the data will be extracted
* returns
  * A Task object for a StandardTableDataCM object

---
**async Task<CrateChooser> GenerateCrateChooser(ActivityDO curActivityDO, string name, string label, bool singleManifest, bool requestUpstream = false, bool requestConfig = false)**

Generic function for creating a CrateChooser which is suitable for most use-cases
* Params
  * ActivityDO activityDO: the ActivityDO object from which the CrateChooser will be generated
  * string name: the name of the CrateChooser object
  * string label: the value to be used for the CrateChooser.Label field
  * bool singleManifest: the value to be used for the CrateChooser.SingleManifestOnly field
  * bool requestUpstream: the value to be used for the CrateChooser.RequestUpstream field. Defaults to false
  * bool requestConfig: if true, a ControlEvent object is added to the CrateChooser.Events list with the name "onChange" and value "requestConfig"
* returns
  * A Task object for a CrateChooser type

---
**void AddTextSourceControl(ICrateStorage storage, string label, string controlName, string upstreamSourceLabel, string filterByTag = "", bool addRequestConfigEvent = false, bool required = false, bool requestUpstream = false)**

Creates StandardConfigurationControlsCM with TextSource control
* Params
  * ICrateStorage crateStorage: the ICrateStorage object to which the TextSource control will be added
  * string label: the label for the TextSourceControl
  * string controlName: the name for the TextSourceControl
  * string upstreamSourceLabel: the label for the upstream source
  * string filterByTag: the tag to use when filtering the upstream sources. Defaults to the empty string
  * bool addRequestConfigEvent: specifies whether to add a request config event to the TextSourceControl. Defaults to false
  * bool required: specifies whether the TextSourceControl is a required value. Defaults to false.
  * bool requestUpstream: specifies the value of the RequestUpstream field

---
**protected TextSource CreateSpecificOrUpstreamValueChooser(string label, string controlName, string upstreamSourceLabel = "", string filterByTag = "", bool addRequestConfigEvent = false, bool requestUpstream = false)**

Creates a TextSourceControl object
* Params
  * string label: the label of the TextSourceControl object
  * string controlName: the name of the TextSourceControl object
  * string upstreamSourceLabel: the label for the TextSourceControl object. Defaults to the empty string
  * string filterByTag: the value to use when setting the TextSourceControl.FilterByTag field. Defaults to the empty string.
  * bool addRequestConfigEvent: specifies whether to add the requestConfig ControlEvent to the TextSourceControl.Events list. Defaults to false
  * bool requestUpstream: value to use when setting the TextSourceControl.RequestUpstream field. Defaults to false.
* returns
  * A newly created TextSourceControl object

---
**UpstreamCrateChooser CreateUpstreamCrateChooser(string name, string label, bool isMultiSelection = true)**

Creates an UpstreamCrateChooser object
* Params
  * string name: the name of the UpstreamCrateChooser
  * string label: the human-friendly label for the CrateChooser
  * bool isMultiSelection: specifies the value for the UpstreamCrateChooser.MultiSelection field. Defaults to true.
* returns
  * A newly created UpstreamCrateChooser object

---
**TextBlock GenerateTextBlock(string curLabel, string curValue, string curCssClass, string curName = "unnamed")**

Creates a TextBlock control
* Params
  * string curLabel: the human-friendly label of the TextBlock
  * string curValue: the value of the TextBlock.Value field
  * string curCssClass: the value of the TextBlock.CssClass field
  * string curName: the value of the TextBlock.Name field. Defaults to "unnamed"
* returns
  * A TextBlock control object

---
**StandardConfigurationControlsCM GetControlsManifest(ActivityDO curActivity)**

Looks for the Configuration Controls Crate and Extracts the ManifestSchema
* Params
  * ActivityDO activityDO: The ActivityDO from which the StandardConfigurationControlsCM should be extracted
* returns
  * A newly created StandardConfigurationControlsCM object with fields from the ActivityDO parameter

---
**ControlDefinitionDTO GetControl(StandardConfigurationControlsCM controls, string name, string controlType = null)**
_deprecated_

Retrieves the control with the specified name from the StandardConfigurationControlsCM parameter
* Params
  * StandardConfigurationControlsCM controls: the CrateManifest which contains the controls from which the ControlDefinitionDTO will be extracted
  * string name: the name of the Control to retrieve
  * string controlType: the type of the Control to extract. Defaults to null.
* returns
  * The ControlDefinitionDTO from the controls parameter with the name specified by the name parameter

---
**T GetControl<T>(StandardConfigurationControlsCM controls, string name, string controlType = null) where T : ControlDefinitionDTO**
_deprecated_

Generic method for retrieving a Control from a StandardConfigurationControlsCM object
* Params
  * StandardConfigurationControlsCM controls: the CrateManifest which contains the controls from which the ControlDefinitionDTO will be extracted
  * string name: the name of the Control to retrieve
  * string controlType: the type of the Control to extract. Defaults to null.
* returns
  * A Control object from the controls parameter with the name specified by the name parameter whose type is specified by the generic type parameter

---
**ICrateStorage AssembleCrateStorage(params Crate[] curCrates)**
_deprecated_

Creates a new CrateStorage object from the curCrates parameter
* Params
  * Crate[] curCrates: an array of Crates to be added to a CrateStorage object
* returns
  * A new CrateStorage object whose Crates field is set to the value of the curCrates parameter

---
**Crate PackControls(StandardConfigurationControlsCM page)**
_deprecated_

Helper method which calls PackControlsCrate
* Params
  * StandardConfigurationControlsCM page: the container of Controls which will be packed
* returns
  * A Crate object of type StandardConfigurationControlsCM

---
**Crate<StandardConfigurationControlsCM> PackControlsCrate(params ControlDefinitionDTO[] controlsList)**
_deprecated_

Creates a Crate of StandardConfigurationControlsCM type
* Params
  * ControlDefinitionDTO[] controlsList: An array of ControlDefinitionDTO objects which will be added to a Crate of StandardConfigurationControlsCM type
* returns
  * A Crate object of type StandardConfigurationControlsCM

---
**string ExtractControlFieldValue(ActivityDO curActivityDO, string fieldName)**
_deprecated_

Extracts the value of the Control whose name is equal to the fieldName parameter from the curActivityDO parameter
* Params
  * ActivityDO activityDO: the ActivityDO whose Control objects will be searched for the fieldName
  * string the name of the field to extract
* returns
  * A string representing the value of the Control object whose name is equal to fieldName

---
**void AddLabelControl(ICrateStorage storage, string name, string label, string text)**
_deprecated_

Adds a label control to ICrateStorage object specified by the storage parameter
* Params
  * ICrateStorage storage: the CrateStorage object to which the label control will be added
  * string name: the name of the label control
  * string label: the human-friendly label for the control
  * string text: the value of the generated label control

---
**void AddControl(ICrateStorage storage, ControlDefinitionDTO control)**
_deprecated_

Adds a control to the CrateStorage object specified by the storage parameter
* Params
  * ICrateStorage storage: the CrateStorage object to which the parameter will be added
  * ControlDefinitionDTO control: the control object to add to the CrateStorage

---
**void InsertControlAfter(ICrateStorage storage, ControlDefinitionDTO control, string afterControlName)**
_deprecated_

Inserts a Control in to a Storage after the control with the name specified by the afterControlName parameter
* Params
  * ICrateStorage storage: the CrateStorage to which the Control will be added
  * ControlDefinitionDTO control: the control which will be added to the CrateStorage
  * string afterControlName: the name of the Control which will precede the new control

---
**ControlDefinitionDTO FindControl(ICrateStorage storage, string name)**
_deprecated_

Finds a Control specified by the name parameter in the ICrateStorage object
* Params
  * ICrateStorage storage: the CrateStorage object which contains the Control
  * string name: the name of the Control to find
* returns
  * ControlDefinitionDTO object, if found in the ICrateStorage parameter

---
**void SetControlValue(ActivityDO activity, string controlFullName, object value)**
_deprecated_

Sets the value of the control specified by the controlFullName parameter in the ActivityDO object
* Params
  * ActivityDO activity: The ActivityDO object which contains the Control whose value will be set
  * string controlFullName: the name of the Control whose value should be set
  * object value: the value to set on the Control

---
**ControlDefinitionDTO TraverseNestedControls(List<ControlDefinitionDTO> controls, string childControl)**
_deprecated_

Iterates over the Control objects and finds the nested controls
* Params
  * List<ControlDefnitionDTO> controls: the list of Control objects to iterate over
  * string childControl: a list of names for the Control objects
* returns
  * The most nested ControlDefinitionDTO object

---
**void RemoveControl(ICrateStorage storage, string name)**
_deprecated_

Removes a Control with the specified name from the ICrateStorage specified by the storage parameter
* Params
  * ICrateStorage storage: the ICrateStorage object which contains the Control which should be removed
  * string name: the name of the Control to remove

---
**Crate<StandardConfigurationControlsCM> EnsureControlsCrate(ICrateStorage storage)**
_deprecated_

Initializes a Crate of StandardConfigurationControlsCM
* Params
  * ICrateStorage storage: the storage to which the Crate should be added
* return
  * A Crate object of type StandardConfigurationControlsCM

---
**string ParseConditionToText(List<FilterConditionDTO> filterData)**
_deprecated_

Parses the comparison condition of the FilterConditionDTO and returns a string representation
* Params
  * List<FilterConditionDTO> filterData: the list of FilterConditionDTO objects from which the condition should be parsed
* returns
  * a string representation of the condition

---
**StandardConfigurationControlsCM GetConfigurationControls(ActivityDO curActivityDO)**
_deprecated_

Gets the StandardConfigurationControlsCM from the ActivityDO object
* Params
  * ActivityDO curActivityDO: the ActivityDO from which the configuration controls will be extracted
* returns
  * A StandardConfigurationControlsCM object with the controls extracted from the ActivityDO

---
**StandardConfigurationControlsCM GetConfigurationControls(ICrateStorage storage)**
_deprecated_

Extract the StandardConfigurationControlsCM from an ICrateStorage object
* Params
  * ICrateStorage storage: the ICrateStorage object from which the configuration controls should be extracted
* returns
  * A StandardConfigurationControlsCM object

---
**int GetLoopIndex(OperationalStateCM operationalState)**
_deprecated_

Get the loop index from the OperationalStateCM
* Params
  * OperationalStateCM operationalState: the OperationalStateCM from which the loop index should be extracted
* returns
  * the integer value of the loop index

---
**object GetCurrentElement(IEnumerable<object> enumerableObject, int objectIndex)**
_deprecated_

Returns the element at the index
* Params
  * IEnumerable<object> enumerableObject: the IEnumerable object from which the element should be extracted
  * int objectIndex: the index of the object to get
* returns
  * an object from the enumerableObject

---
**OperationalStateCM GetOperationalStateCrate(ICrateStorage storage)**
_deprecated_

Retrieves and OperationalStateCM from the ICrateStorage object
* Params
  * ICrateStorage storage: the storage from which the OperationalStageCM should be extracted
* returns
  * An OperationalStateCM object, if one exists