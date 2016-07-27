# EVENTS

[Go to Contents](/Docs/Home.md)

## Summary

Terminals often needs to communicate with hub. Some examples include getting payload, running a plan or creating an activity. There are certain endpoints on hub which are allowed to be called by terminals.

## Authentication with Hub

All terminals needs to authenticate to communicate with hub. With every request to terminals, hub includes 3 headers which are required to authenticate with hub. Those headers are;

* **Fr8HubCallbackSecret**: 4b54d12f7f834648be28aa247f523e21
* **Fr8HubCallBackUrl**: http://dev.fr8.co/
* **Fr8UserId**: d4991c09-77ee-42de-9ae7-15c1b6c2d3ca

### Fr8HubCallbackSecret

This header contains the secret key for your terminal. When you need to communicate back with hub you will need this secret as your terminal identifier.

### Fr8HubCallBackUrl

This header contains the url of the hub which is making request to your terminal. All your communications should be made with this hub.

Fr8 is a distributed environment. Your terminal might be in use by many hubs. Therefore this header contains url of the current hub which is making the request.

### Fr8UserId

This header contains the id of the user. Current request to your terminal is made on behalf of this user.

### Authentication

When your terminal needs to make a request to hub. It needs to add FR8-TOKEN Authorization header to it's request.

Header value is created using the following format (without quotes): "FR8-TOKEN key={Fr8HubCallbackSecret}, user={Fr8UserId}"

Here is a an example text of required request headers:

	Authorization: FR8-TOKEN key=2db48191-cda3-4922-9cc2-a636e828063f, user=76de71f2-f346-4bc9-96e0-f7bd1c87a575

## Hub Endpoints

Here is a list of hub endpoints that can be called by terminals.

### 1. CreatePlan

This endpoint is used to create plans by terminals or activities. Generally used to create automatic monitoring plans upon successful authentication to external systems.

*Url*

    {{Fr8HubCallBackUrl}}/api/{{Fr8HubApiVersion}}/plans
*Method*

    POST
*Request Body*


```javascript
{
  "id": "REMOVE THIS PROPERTY",
  "name": "My test plan",
  "tag": "Plan tags - REMOVE THIS?",
  "description": "Description of my test plan",
  "lastUpdated": "REMOVE THIS PROPERTY",
  "planState": "REMOVE THIS PROPERTY",
  "startingSubPlanId": "REMOVE THIS PROPERTY",
  "visibility": 1,
  "category": "REMOVE THIS?"
}
```

*Response Body*
```javascript
{
  "id": "123456789",
  "name": "My test plan",
  "tag": "Plan tags - REMOVE THIS?",
  "description": "Description of my test plan",
  "lastUpdated": "Date",
  "planState": 1,
  "startingSubPlanId": "123456",
  "visibility": 1,
  "category": "REMOVE THIS?",
  "subPlans": [{
      "activities": [],
      "subPlanId": "123456789",
      "planId": "1234560123",
      "parentId": "1234560123",
      "name": null,
      "transitionKey": "string"
      "runnable": true
  }],
  "fr8UserId": "12345678901"
}
```

Since we just created this plan, it doesn't have any activities yet.

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




### RunPlan

### LoadPlan

### GetPlansByName

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
