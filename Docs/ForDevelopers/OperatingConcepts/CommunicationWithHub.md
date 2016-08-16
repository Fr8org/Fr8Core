# Hub Communication

[Go to Contents](/Docs/Home.md)

## Summary

Terminals often needs to communicate with hub. Some examples include getting payload, running a plan or creating an activity. There are certain endpoints on hub which are allowed to be called by terminals.

Basics of Terminal Authentication are covered [here](/Docs/ForDevelopers/OperatingConcepts/Authorization/TerminalAuthentication.md)


## Hub Endpoints

Here is a list of hub endpoints that can be called by terminals.

### 1. CreatePlan

This endpoint is used to create plans by terminals or activities. Generally used to create automatic monitoring plans upon successful authentication to external systems.

***API Definition:*** https://fr8.co/swagger/ui/index#!/Plans/Plans_Post

Here are some additional notes about Create Plan that aren't covered in the swagger definition.

Note: id, lastUpdated, planState and startingSubPlanId properties shouldn't live inside PlanEmptyDTO - move them to PlanFullDTO instead

Note2: we kind of never use category on plans. are we planning to?
Note: we shouldn't receive plan state from terminals or users. This is our internal property.

#### Available Plan States

    Inactive = 1,
    Running = 2,
    Deleted = 3

#### Available Visibility Values

    Standard = 1,
    Internal = 2

Internal plans are hidden from users plan list. They are generally used to create monitoring plans which automatically records user data. See [MADSE Plan](/Docs/ForDevelopers/Samples/MADSEPlan.md)


### 2. RunPlan

This endpoint is used to trigger a plan run from a terminal. You might pass a payload to this endpoint to start plan execution with a predefined payload.

***API Definition:*** https://fr8.co/swagger/ui/index#!/Plans/Plans_Run


### 3. LoadPlan

This endpoint loads specified plan from the hub.

***API Definition:*** https://fr8.co/swagger/ui/index#!/Plans/Plans_Load

Note: it seems this endpoint's purpose was changed to load a plan template. Currently it is not in use by any .net terminals. i removed this from HubCommunicator. We probably need to close this endpoint to terminals.

### 4. GetPlans

This endpoint loads plans by given name or by given activity id. Generally used to check if an auto created plan already exists in the Hub or to get owner plan of an activity.

***API Definition:*** https://fr8.co/swagger/ui/index#!/Plans/Plans_Get

Note: This endpoint is confusing and we probably should refactor our codes to use GetPlansByQuery endpoint. There 2 different get plans endpoints. we should DRY it


### 5. DeletePlan

This endpoint deletes given plan by id.

***API Definition:*** https://fr8.co/swagger/ui/index#!/Plans/Plans_Delete

### 6. UpdateCreatePlan

This endpoints updates given plan or it creates a new plan. If you pass a solutionName to this endpoint, it creates a solution.

***API Definition:*** https://fr8.co/swagger/ui/index#!/Plans/Plans_Post

Note: we should update this endpoint. it should just create the given plan. We should have a different endpoint for updating the plan.
Multi purpose endpoints are confusing.

And we shouldn't make users post LastUpdated property. This is for our internal usage. we should simplify API dtos
We kind of never use categories on plans. we probably should remove them.

#### Tags

Tags lets terminals to tag their auto created plans. This is useful to check if an auto plan was already created on the Hub.

#### Available Plan States

    "Inactive",
    "Active"

Note: we are sometimes receiving plan state as int and sometimes string. we should fix that.
Note: We shouldn't allow plan states modified externally. this is for internal usage. we should remove that.


### 7. GetActivityTemplates

This endpoint lists the Activity Templates in their corresponding categories.

***API Definition:*** https://fr8.co/swagger/ui/index#!/ActivityTemplates/ActivityTemplates_Get_0

Note: This endpoint gives duplicate activity template data. Currently categories contain activities and activities contain categories. this endpoint should be simplified to give a list of activity templates (templates should contain their categories)

### 8. ConfigureActivity

This endpoint requests hub to configure an activity.

***API Definition:*** https://fr8.co/swagger/ui/index#!/Activities/Activities_Configure

Note: we should probably rename parameters for this endpoint. curActionDesignDTO isn't really helpful. And maybe we should simplify required data for this endpoint. only crateStorage and activity id should be enough to configure an activity. Currently we are asking for a complete activityDTO which includes some unnecessary stuff like planid and etc.

### 9. SaveActivity

This endpoint save modifications to an activity to the Hub.

***API Definition:*** https://fr8.co/swagger/ui/index#!/Activities/Activities_Save

Note: rename parameters

### 10. CreateActivity

This endpoint creates and configures given activity.

***API Definition:*** https://fr8.co/swagger/ui/index#!/Activities/Activities_Create

### 11. DeleteActivity

This endpoint deletes given activity. This endpoint can also be used to delete only child activities of current activity by setting delete_child_nodes flag.

***API Definition:*** https://fr8.co/swagger/ui/index#!/Activities/Activities_Delete

### 12. GetPayload

This endpoint gets current container's payload from the Hub.

***API Definition:*** https://fr8.co/swagger/ui/index#!/Containers/Containers_payload

### GetTokens

This endpoint gets all tokens of specified user.

***API Definition:*** https://fr8.co/swagger/ui/index#!/Authentication/Authentication_tokens

Note: really??? this is a major security flaw

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
