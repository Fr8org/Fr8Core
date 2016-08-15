# Hub Communication

[Go to Contents](/Docs/Home.md)

## Summary

Terminals often needs to communicate with hub. Some examples include getting payload, running a plan or creating an activity. There are certain endpoints on hub which are allowed to be called by terminals.


## Hub Endpoints

Here is a list of hub endpoints that can be called by terminals.

### 1. CreatePlan

This endpoint is used to create plans by terminals or activities. Generally used to create automatic monitoring plans upon successful authentication to external systems.

***API Definition:*** https://fr8.co/swagger/ui/index#!/Plans/Plans_Post

Here are some additional notes about Create Plan that aren't covered in the swagger definition.

Note: id, lastUpdated, planState and startingSubPlanId properties shouldn't live inside PlanEmptyDTO - move them to PlanFullDTO instead

Note2: we kind of never use category on plans. are we planning to?

#### Available Plan States

    Inactive = 1,
    Running = 2,
    Deleted = 3

#### Available Visibility Values

    Standard = 1,
    Internal = 2

Internal plans are hidden from users plan list. They are generally used to create monitoring plans which automatically records user data. See [MADSE Plan](/Docs/ForDevelopers/Samples/MADSEPlan.md)


### 2. RunPlan

This endpoint is used to trigger a plan run from a terminal.
***API Definition:*** https://fr8.co/swagger/ui/index#!/Plans/Plans_Run


### 3. LoadPlan

This endpoint loads specified plan from the hub.
***API Definition:*** https://fr8.co/swagger/ui/index#!/Plans/Plans_Load

Note: it seems this endpoint's purpose was changed to load a plan template. Currently it is not in use by any .net terminals. i removed this from HubCommunicator. We probably need to close this endpoint to terminals.

### 4. GetPlansByName

Note: This endpoint is confusing



### DeletePlan

### GetPlansByActivity

### UpdatePlan


### GetActivityTemplates


### ConfigureActivity

### SaveActivity

### CreateAndConfigureActivity

### DeleteActivity

### DeleteExistingChildNodesFromActivity


### GetPayload

### GetTokens

### GetCurrentUser

### GetAvailableData

### GetCratesByDirection


### CreateAlarm

### ScheduleEvent


### SaveFile

### DownloadFile

### GetFiles


### GetStoredManifests

### ApplyNewToken

### NotifyUser

### RenewToken

### SendEvent

### QueryWarehouse

### AddOrUpdateWarehouse

### DeleteFromWarehouse

[Go to Contents](/Docs/Home.md)
