# TerminalActivityBase

The most low level base class for developing activities. 

**Namespace**: Fr8.TerminalBase.BaseClasses  
**Assembly**: Fr8TerminalBase.NET


## Properties
| Name                            |Description                                                                                 |
|---------------------------------|------------------------------------------------------------------------------------------- |
| Payload | Crate storage with the payload from the currently processing container.  You can modify this information. All changes will be persisted by the Hub. `Note: this property is not available at the design-time.`|
| OperationalState| Operatial state from the currently processing container. `Note: this property is not available at the design-time.`|
| ExecutionContext| Full information about currently executing container. `Note: this property is not available at the design-time.`|
| Storage |Crate storage with configuration data of the current activity. You can modify this information. All changes will be persisted by the Hub if they were done during the design-time. Changes made during execution will be discarded. |
| ActivityPayload |Information about the current activity. This information includes name, children activities, etc. You can modify this information. All changes will be persisted by the Hub if they were done during the design-time. Changes made during execution will be discarded.|
| AuthorizationToken |Authorization token that was passed by the Hub for processing the current request. Can be null.|
| HubCommunicator | IHubCommunicator instance that can be used to work with the Hub that has made current request.|
| ActivityContext| Full information about current activity configuration and security information related to the current request.|
| IsRuntime| Read-only flag showing if current request is related to activity execution. |
| AuthenticationMode| Use this property to set or get authorization mode for the current activity instance. By default **AuthorizationMode** is set basing on the current terminal's **AuthenticationType** property |
| DisableValidationOnFollowup| Use this flag to get or set if validation should be automatically triggered before follow-up configuration request processing. |
| CrateManager| Current ICrateManager instance. |
| [ValidationManager](/Docs/ForDevelopers/SDK/.NET/Reference/ValidationManager.md)| Validation service configured to work with the current activity. |
| ActivityId| Current activity Id |
| CurrentUserId| Current user Id |
| UiBuilder| Instance of UiBuilder service configured to work with the current activity |
| MyTemplate| Specifies activity template for the current activity |
| [CrateSignaller](/Docs/ForDevelopers/SDK/.NET/Reference/CrateSignaller.md)| CrateSignaller services configured to work with the current activity |
| this[string] | Read-write accessor to embedded key-value storage. All data in this storage will be persisted forever. You can use this to store some auxiliary configuration data that doesn't worth creating dedicated crate. |




## Methods
| Name                            |Description                                                                                 |
|---------------------------------|------------------------------------------------------------------------------------------- |
| CheckAuthentication()  |  Checks if authentication is needed. Should return **true** if authentication is required otherwise - **false**. Default implementation consider authentication is required in case when current activity's ActivityTemplate has **NeedsAuthentication** is set to **true** and no authorization token information was passed within the current request. Override this method if you need more sophisticated checks, like token validation. This method is invoked before executing any other request processing logic. If this method returns **false** and the current request is **/configure** **StandardAuthenticationCM** crate is added to activity storage and request processing is aborted. In case of **/run** an error is generated and request is aborted.  |
| AddAuthenticationCrate(bool) | Adds **StandardAuthenticationCM** crate to activity storage. |
| Initialize() | Override this method to implement initial configuration logic| 
| FollowUp() | Override this method to implement follow-up configuration logic| 
| Run() | Override this method to implement activity execution logic| 
| RunChildActivities() | Override this method to implement activity execution logic after all child activities have been executed| 
| Activate() | Override this method to implement activity activation logic | 
| Deactivate() | Override this method to implement activity deactivation logic |
| GetConfigurationRequestType ()| Override this method if you need custom way to determine the type of configuration request. By default, if activity storage is empty, request is considered to be **Initial**, otherwise - **Follow-up**| 
| InitializeInternalState() | Override this method if you need to perform some global initialization before any activity logic is executed |
| BeforeRun() | Override this method if need to perform some custom logic before activity execution (Run or RunChildActivities). If this method returns **false** activity execution is terminated. Default implementation performs validation checks and prevents subsequent execution in case of validation errors. |
| AfterRun(Exception) | Override this method if need to perform some custom logic after activity execution (Run or RunChildActivities). This method is called even if Run or RunChildActivities fails with exception. In this case, exception is passed as a parameter of this method. |
| BeforeConfigure(ConfigurationRequestType) | Override this method if need to perform some custom logic before activity configuration (Initialize or Followup). If this method returns **false** activity configuration is terminated. Default implementation performs validation checks and prevents subsequent execution in case of validation errors. |
| AfterConfigure(ConfigurationRequestType, Exception) | Override this method if need to perform some custom logic after activity configuration (Initialize or Followup). This method is called even if Initialize or Followup fails with exception. In this case, exception is passed as a parameter of this method. |
| BeforeActivate() | Override this method if need to perform some custom logic before activity activation. If this method returns **false** activity activation is terminated. |
| AfterActivate(Exception) | Override this method if need to perform some custom logic after activity activation. This method is called even if Activate fails with exception. In this case, exception is passed as a parameter of this method. |
| BeforeDeactivate() | Override this method if need to perform some custom logic before activity deactivation. If this method returns **false** activity deactivation is terminated. |
| AfterDeactivate(Exception) | Override this method if need to perform some custom logic after activity deactivation. This method is called even if Deactivate fails with exception. In this case, exception is passed as a parameter of this method. |
| Validate() | Override this method to implement validation logic. By default, this method is called before activity configuration logic and before activity execution. See **DisableValidationOnFollowup** also.|
| CreateValidationManager() | Factory method for creating validation manager. |
| IsInvalidTokenException(Exception) | Override this method to take advantage of integrated authorization token invalidation check. If one or activity methods throw exception this exception is evaluated using this method. If it returns **true** all possible actions are performed to request token invalidation from the Hub. |
| RequestPlanExecutionSuspension (string) | Pause execution of the current container after current activity finishes execution |
| RequestPlanExecutionTermination (string) | Terminate execution of the current container after current activity finishes execution |
| Success (string) | Report success to the Hub. This is the default response that is send to the Hub upon successful activity execution |
| RequestClientActivityExecution (string) | Request execution of some action from the client. Requested action will be perform by the client only after current activity finishes execution |
| RequestSkipChildren() | After current activity finishes execution, skip execution of children and move container execution pointer to the first sibling of current activity.
| RequestCall(Guid)| After current activity finishes execution, request calling activity or subplan and return container execution pointer back to the current activity
| RequestJumpToActivity (Guid) | After current activity finishes execution, request moving current container execution pointer to activity in same subplan as current activity|
| RequestJumpToSubplan(Guid) | After current activity finishes execution, request moving current container execution pointer to subplan in same plan as current activity|
| RequestLaunchPlan(Guid) | After current activity finishes execution, launch new plan with a given id|
| SetResponse (ActivityResponse, string, string) | Set response code with parameters that will be sent to the Hub after current activity execution |
| RaiseError (string,  ActivityErrorCode, ErrorType) | Report activity execution error |
| RaiseInvalidTokenError (string) | Report invalid token error |
| GetDocumentation (string) | Override this method to return custom documentation for the activity |

## Remarks
While being low-level this class provides all necessary routines to create fully functional activity. The only thing that is not covered in this class is manipulations with **StandardConfigurationControlsCM** crate. While using this class you are free to manipulate with configuration controls without any restrictions. But more practical way is to use these following classes as the base class for new activity depending on your requirements:

* [TerminalActivity\<TUi>](/Docs/ForDevelopers/SDK/.NET/Reference/TerminalActivityT.md) (recommended for new activity development)
* [ExplicitTerminalActivity](/Docs/ForDevelopers/SDK/.NET/Reference/ExplicitTerminalActivity.md)

### Before*** and After*** methods
These methods are useful if you are implementing a new base class atop of TerminalActivityBase. You can perform some specific logic in Before*** and After*** methods while allowing users of your base class to override usual Run, Followup, Activate, etc methods. 

### Validation
Generally, you should not manually check for validation errors before performing configuration or execution logic. You should override **Validate** method instead. Base class logic calls **Validate** by itself and perform all necessary actions including user-friendly error message generation in case of validation errors.