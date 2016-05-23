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
* void SetCurrentUser(string UserId, string UserEmail)

  Sets the CurrentFr8UserId and CurrentFr8UserEmail fields
  * Params
    * string UserId: the id for the current user
    * string UserEmail: the email address for the current user

* PayloadDTO SuspendHubExecution(PayloadDTO payload)

  Creates an OperationalStateCM Crate object with a value of ActivityResponse.RequestSuspend
  * Params
    * PayloadDTO payload: a PayloadDTO object that contains (?)
  * returns
    * PayloadDTO object that contains (?)
* PayloadDTO TerminateHubExecution(PayloadDTO payloadDTO, string message)

  Creates an OperationalStateCM Crate object with a value of ActivityResponse.RequestTerminate
  * Params
    * PayloadDTO payload: a PayloadDTO object which contains (?)
    * string message: a message that is used for (?)
  * returns
    * PayloadDTO object which contains (?)
* PayloadDTO LaunchPlan(PayloadDTO payloadDTO, Guid targetPlanId)

  Creates an OperationalStateCM Crate object with a value of ActivityResponse.LaunchAdditionalPlan
  * Params
    * PayloadDTO payload: a PayloadDTO object which contains (?)
    * Guid targetPlanId: the ID of the plan which should be launched
  * returns
    * PayloadDTO object which contains (?)
* PayloadDTO JumpToSubplan(PayloadDTO payload, Guid targetSubplanId)

  Creates an OperationalStateCM Crate object with a value of ActivityResponse.JumpToSubplan
  * Params
    * PayloadDTO payload: a PayloadDTO object which contains (?)
    * Guid targetSubplanId: the ID of the Subplan which should be executed
  * returns
    * PayloadDTO object which contains (?)
* PayloadDTO JumpToActivity(PayloadDTO payload, Guid targetActivityId)

  Creates an OperationalStateCM Crate object with a value of ActivityResponse.JumpToActivity
  * Params
    * PayloadDTO payload: A PayloadDTO obejct which contains (?)
    * Guid targetActivityId: the ID of the activity which should be moved to
  * returns
    * PayloadDTO object which contains (?)
