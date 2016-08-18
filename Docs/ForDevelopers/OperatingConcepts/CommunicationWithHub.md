# Hub Communication

[Go to Contents](/Docs/Home.md)

## Summary

Terminals often needs to communicate with hub. Some examples include getting payload, running a plan or creating an activity. There are certain endpoints on hub which are allowed to be called by terminals.

Basics of Terminal Authentication are covered [here](/Docs/ForDevelopers/OperatingConcepts/Authorization/TerminalAuthentication.md)


## Hub Endpoints

Here is a list of hub endpoints that can be called by terminals.

## 1. Plan Endpoints

### 1.1 CreatePlan

This endpoint is used to create plans by terminals or activities. Generally used to create automatic monitoring plans upon successful authentication to external systems.

[**API Definition**](https://fr8.co/swagger/ui/index#!/Plans/Plans_Post)

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


### 1.2. RunPlan

This endpoint is used to trigger a plan run from a terminal. You might pass a payload to this endpoint to start plan execution with a predefined payload.

[**API Definition**](https://fr8.co/swagger/ui/index#!/Plans/Plans_Run)


### 1.3. LoadPlan

This endpoint loads specified plan from the hub.

[**API Definition**](https://fr8.co/swagger/ui/index#!/Plans/Plans_Load)

Note: it seems this endpoint's purpose was changed to load a plan template. Currently it is not in use by any .net terminals. i removed this from HubCommunicator. We probably need to close this endpoint to terminals.

### 1.4. GetPlans

This endpoint loads plans by given name or by given activity id. Generally used to check if an auto created plan already exists in the Hub or to get owner plan of an activity.

[**API Definition**](https://fr8.co/swagger/ui/index#!/Plans/Plans_Get)

Note: This endpoint is confusing and we probably should refactor our codes to use GetPlansByQuery endpoint. There 2 different get plans endpoints. we should DRY it

Note: we aren't using our usual security system here. we need to use ObjectRolePermissions to check if user has read right to this plan


### 1.5. DeletePlan

This endpoint deletes given plan by id.

[**API Definition**](https://fr8.co/swagger/ui/index#!/Plans/Plans_Delete)

### 1.6. UpdateCreatePlan

This endpoints updates given plan or it creates a new plan. If you pass a solutionName to this endpoint, it creates a solution.

[**API Definition**](https://fr8.co/swagger/ui/index#!/Plans/Plans_Post)

Note: we should update this endpoint. it should just create the given plan. We should have a different endpoint for updating the plan.
Multi purpose endpoints are confusing.

And we shouldn't make users post LastUpdated property. This is for our internal usage. we should simplify API dtos
We kind of never use categories on plans. we probably should remove them.

Note: we aren't doing any security checks at all here. anybody who knows the plan id is free to update the plan. we should fix this!

#### Tags

Tags lets terminals to tag their auto created plans. This is useful to check if an auto plan was already created on the Hub.

#### Available Plan States

    "Inactive",
    "Active"

Note: we are sometimes receiving plan state as int and sometimes string. we should fix that.
Note: We shouldn't allow plan states modified externally. this is for internal usage. we should remove that.

## 2. Activity Template Endpoints

### 2.1. GetActivityTemplates

This endpoint lists the Activity Templates in their corresponding categories.

[**API Definition**](https://fr8.co/swagger/ui/index#!/ActivityTemplates/ActivityTemplates_Get_0)

Note: This endpoint gives duplicate activity template data. Currently categories contain activities and activities contain categories. this endpoint should be simplified to give a list of activity templates (templates should contain their categories)

## 3. Activity Endpoints

### 3.1. ConfigureActivity

This endpoint requests hub to configure an activity.

[**API Definition**](https://fr8.co/swagger/ui/index#!/Activities/Activities_Configure)

Note: we should probably rename parameters for this endpoint. curActionDesignDTO isn't really helpful. And maybe we should simplify required data for this endpoint. only crateStorage and activity id should be enough to configure an activity. Currently we are asking for a complete activityDTO which includes some unnecessary stuff like planid and etc. Should user be able to update those properties during configure?

### 3.2. SaveActivity

This endpoint save modifications to an activity to the Hub.

[**API Definition**](https://fr8.co/swagger/ui/index#!/Activities/Activities_Save)

Note: rename parameters

### 3.3. CreateActivity

This endpoint creates and configures given activity.

[**API Definition**](https://fr8.co/swagger/ui/index#!/Activities/Activities_Create)

### 3.4. DeleteActivity

This endpoint deletes given activity. This endpoint can also be used to delete only child activities of current activity by setting delete_child_nodes flag.

[**API Definition**](https://fr8.co/swagger/ui/index#!/Activities/Activities_Delete)

## 4. Container Endpoints

### 4.1. GetPayload

This endpoint gets current container's payload from the Hub.

[**API Definition**](https://fr8.co/swagger/ui/index#!/Containers/Containers_payload)

## 5. Authentication Endpoints

### 5.1. GetTokens

This endpoint gets all tokens of specified user.

[**API Definition**](https://fr8.co/swagger/ui/index#!/Authentication/Authentication_tokens)

Note: really??? this is a major security flaw. A terminal can easily get all tokens of a user

### 5.2. ApplyNewToken

This endpoint sets the Authorization Token for list of activities.

[**API Definition**](https://fr8.co/swagger/ui/index#!/Authentication/Authentication_GrantTokens_0)

### 5.3. RenewToken

This endpoint updates an existing authentication token with a new one for given activity.

[**API Definition**](https://fr8.co/swagger/ui/index#!/Authentication/Authentication_RenewToken)

Note: we require a AuthenticationTokenDTO for this endpoint. it contains too much unnnecessary information (domain name of a token is unlikely to change this way). this endpoint should only contain activity id and new token. We should simplify this.

## 6. User Endpoints

### 6.1. GetCurrentUser

This endpoint gives you the information about current user, that your terminal is operating on.

[**API Definition**](https://fr8.co/swagger/ui/index#!/Users/Users_UserData)

Note: we are giving way too much information here. Terminals shouldn't know about user's profile id or email id. we should simplify this.

## 7. Plan Nodes Endpoints

### 7.1. GetAvailableData

This endpoint lets you query upstream or downstream signalled crates and fields relative to your activity by their availability types.

[**API Definition**](https://fr8.co/swagger/ui/index#!/PlanNodes/PlanNodes_signals)

#### Available Directions

      Upstream = 0,
      Downstream = 1,
      Both = 2

#### Available Availability Types

      NotSet = 0,
      Configuration = 0x1,
      RunTime = 0x2,
      Always = Configuration | RunTime


### 7.2. GetCratesByDirection

This endpoint lets you query entire activities by direction relative to your activity.

[**API Definition**](https://fr8.co/swagger/ui/index#!/PlanNodes/PlanNodes_Get)

#### Available Directions

      "upstream",
      "downstream"

Note: on previous endpoint we are receiving direction as an enum. on this one we are receiving it as string. we should decide a common method for those stuff.

## 8. Alarms Endpoints

### 8.1. CreateAlarm

This endpoint lets you pause current container's execution. This should only be called during runtime. Your container resumes at given start time.

[**API Definition**](https://fr8.co/swagger/ui/index#!/Alarms/Alarms_Post)

Note: we should rename this endpoint to match it's logic. Note this endpoint should verify that given container id exists.

### 8.2. ScheduleEvent

This endpoint lets you create a timer for your terminal's polling endpoint. A terminal can ask the Hub to trigger this endpoint on given intervals. This is generally used to check an external system with periods.

Note that your terminal should have 'polling' endpoint to use this endpoint.

[**API Definition**](https://fr8.co/swagger/ui/index#!/Alarms/Alarms_Polling)

Note: this endpoint feels complicated. PollingDataDTO has too much information in it. we should simplify it. For example: Why do we need user to post RetryCount this is for internal usage.

## 9. File Endpoints

### 9.1. SaveFile

This endpoint lets you save a file on behalf of current user to the Hub's file storage. You should post your file as multipart/form-data.

[**API Definition**](https://fr8.co/swagger/ui/index#!/Files/Files_Post)

Note: we are returning stuff like DockyardAccountId, LastUpdated and etc here. why? we should inspect and simplify this.

### 9.2. DownloadFile

This endpoint lets you download a file that was already save to the Hub's file storage.

[**API Definition**](https://fr8.co/swagger/ui/index#!/Files/Files_Get)

### 9.3. GetFiles

This endpoint gives you a list of current user's files in the Hub file storage.

[**API Definition**](https://fr8.co/swagger/ui/index#!/Files/Files_Get_0)

## 10. Notification Endpoints

### 10.1. NotifyUser

This endpoint triggers a notification which is shown to current user. For more information go to [Notifications](/Docs/ForDevelopers/Services/Notifications.md) page.

[**API Definition**](https://fr8.co/swagger/ui/index#!/Notifications/Notifications_Post)

## 11. Event Endpoints

### 11.1. SendEvent

This endpoint sends an event crate to the Hub which triggers the execution of related plans. For more information go to [Events](/Docs/ForDevelopers/OperatingConcepts/Events.md)

[**API Definition**](https://fr8.co/swagger/ui/index#!/Events/Events_Post)

## 12. Warehouse Endpoints

### 12.1. QueryWarehouse

This endpoint queries data from the Hub's warehouse.

[**API Definition**](https://fr8.co/swagger/ui/index#!/Warehouses/Warehouses_Query)

### 12.2. AddOrUpdateWarehouse

This endpoint add or updates existing data to the Hub's warehouse.

[**API Definition**](https://fr8.co/swagger/ui/index#!/Warehouses/Warehouses_Post)

### 12.3. DeleteFromWarehouse

This endpoint removes existing data from the Hub's warehouse.

[**API Definition**](https://fr8.co/swagger/ui/index#!/Warehouses/Warehouses_Delete)


### xx. GetStoredManifests

This endpoint queries the current user's manifests in the Hub.

Note: This endpoint on HubCommunicator is probably obsolete. it is never used anywhere. we should remove this.

[Go to Contents](/Docs/Home.md)
