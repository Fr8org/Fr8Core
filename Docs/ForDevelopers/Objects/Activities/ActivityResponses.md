

## Activity Responses

When a Terminal completes processing of an Activity, it signals status via Activity Response structure, which is part of the Http response to the Hub.
Hubs are aware of the following Activity Responses:

Value |	What the Hub Does |	Discussion
------ | -------- | ------------------
Success |	If in Design-Mode, returns the response to the Client. If in Run-Mode, executes the next Activity in the Plan. |	The default response.
Error |	Returned when unexpected errors occur. |	 Will generally stop execution of a Plan. Can include a text string that is displayed on the Client.
RequestSuspend |	Run-Time Only. Requests Hub to Pause execution of the Container’s Plan. |	Activities are expected to return a response of some sort within 5 seconds at Run-Time. If they need more time than that they should return  this response.  Hubs receiving this response are expected to pause Container execution and set ContainerState to “WaitingOnAction”
RequestTerminate |	Run-Time Only. Requests Hub to Pause execution of the Container’s Plan. |	Some Activities may determine that no further processing of the Plan is appropriate. They return RequestTerminate. A use case for this is the Filter action, which basically allows an arbitrary filter to be applied to the run-time data based on fields made available at design time. If the filter test fails, we want the entire Container flow terminated. But this isn’t an error scenario. If, for example, we have a route that wants to work only with DocuSign events of a certain priority, we need to terminate all of the Containers that are created for events which don’t have that priority. ContainerState is changed to “Terminated”.
SkipChildren |	Hub skips processing of child actions. |	Used to enable programmatic looping.
Null |	Until all actions start using return codes, Null will be default return code and will be interpreted as success by hub. |	Deprecated.
ExecuteClientAction |	See [Client-Driven Actions](/Docs/ForDevelopers/OperatingConcepts/Client-DrivenActions.md) |	Requests that theClient carry out a Client-Driven Action. These are things that Clients are expected to know how to do. The initial use case is to display data in a tabular report.
JumpToActivity | Requests Hub to move current container execution pointer to activity in same subplan as current activity. | Used to implement condition container execution. |
JumpToSubplan | Requests Hub to move current container execution pointer to subplan in same plan as current activity. | Used to implement condition container execution. |
LaunchAdditionalPlan | Request Hub to run new Plan | |
CallAndReturn | Requests Hub to execute activity or subplan and return container execution pointer back to the current activity | Used to implement condition container execution. |
Break | Request Hub to stop execution of activities within the current parent, and move container execution pointer to the parent's first sibling. It there is no siblings container exectution is terminated | Used to implement condition container execution. Can be used to break Loop execution. |

## ActivityResponse JSON

The ActivityResponse element is intended to be simple and to be insertable into HTTP Responses. For V1, however, the ActivityResponse JSON is written into the Container’s OperationalState Crate. This will change.  

Name | Data Type | Description
--- | --- | ---
Type |	string |	One of the string values in the table above
Body |	string |	A JSON element of arbitrary complexity. Most ActivityResponses do not currently need or use this. A key exception is Error, which uses this to return Error explanation text.
