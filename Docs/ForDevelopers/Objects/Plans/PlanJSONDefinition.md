# PLAN – JSON DEFINITION
[Go to Contents](/Docs/Home.md)  

In Fr8 plans are represented in the form of JSON. Lets looks at the following plan:

![plan with loop](/Docs/img/PlanExecution-PlanWithLoop.png)


The JSON representation of this plan is:
```javascript
{
  "subPlans": [
    {
      "activities": [
        {
          "label": null,
          "name": "Get Google Sheet Data",
          "activityTemplate": {
            "name": "Get_Google_Sheet_Data",
            "version": "1",
            "terminalName": "terminalGoogle",
            "terminalVersion": "1"
          },
          "planId": "5c1499ce-4b21-4ca3-ab81-6924e0d55a84",
          "parentPlanNodeId": "e26e374e-2691-4625-89fe-79d5200b4e89",
          "currentView": null,
          "ordering": 1,
          "id": "28b93b3d-1abe-4443-aa1d-5bfa15365e3e",
          "crateStorage": {
           ... Content is omitted for clarity ...
          },
          "childrenActivities": [],
          "authTokenId": "4e58046e-efb9-4a43-ad93-4a98ad9cc1c0",
          "authToken": null,
          "documentation": null
        },
        {
          "label": null,
          "name": "Loop",
          "activityTemplate": {
            "name": "Loop",
            "version": "1",
            "terminalName": "terminalFr8Core",
            "terminalVersion": "1"
          },
          "planId": "5c1499ce-4b21-4ca3-ab81-6924e0d55a84",
          "parentPlanNodeId": "e26e374e-2691-4625-89fe-79d5200b4e89",
          "currentView": null,
          "ordering": 2,
          "id": "e81fade0-5332-416c-9da1-31a322a70f05",
          "crateStorage": {
           ... Content is omitted for clarity ...
            ]
          },
          "childrenActivities": [
            {
              "label": null,
              "name": "Publish To Slack",
              "activityTemplate": {
                "name": "Publish_To_Slack",
                "version": "2",
                "terminalName": "terminalSlack",
                "terminalVersion": "1"
              },
              "planId": "5c1499ce-4b21-4ca3-ab81-6924e0d55a84",
              "parentPlanNodeId": "e81fade0-5332-416c-9da1-31a322a70f05",
              "currentView": null,
              "ordering": 1,
              "id": "9ad788eb-6ca0-42ee-888d-242d6971d7d8",
              "crateStorage": {
                ... Content is omitted for clarity ...
              },
              "childrenActivities": [],
              "authTokenId": "c8c69cc4-97a1-4787-ac69-1dcc8b9cb083",
              "authToken": null,
              "documentation": null
            }
          ],
          "authTokenId": null,
          "authToken": null,
          "documentation": null
        }
      ],
      "id": "e26e374e-2691-4625-89fe-79d5200b4e89",
      "planId": "5c1499ce-4b21-4ca3-ab81-6924e0d55a84",
      "parentPlanNodeId": null,
      "name": "Starting Subplan"
    }
  ],
  "ownerId": "fbb00d37-5c03-4296-9569-a32aeab70443",
  "id": "5c1499ce-4b21-4ca3-ab81-6924e0d55a84",
  "name": "Plan with loops",
  "tag": null,
  "description": null,
  "lastUpdated": "2016-08-02T19:52:46.2268865+00:00",
  "planState": "Inactive",
  "startingSubPlanId": "e26e374e-2691-4625-89fe-79d5200b4e89",
  "visibility": {
    "hidden": false,
    "public": false
  },
  "category": null
}
```

## Plan node JSON
Describes properties of the [plan](/Docs/ForDevelopers/Objects/Plans/Plans.md) itself.  
* **subPlans** - one or more subplans. Each plan must have at least one subplan.
* **ownerId** - Id of the owner user
* **id** - Id of the plan.
* **name** - Name of the plan. Value of this property is shown in the UI.
* **description** - Optional free form text, describing what this plan is doing.
* **lastUpdated** - Last time this plan was changed
* **startingSubPlanId** - Identifier of the subplan, activities of which will be executed when the user requests the plan execution.
* **planState** - state of the plan. Can be:   
	* **Inactive**. Plan is created, but it is not being executed right now
    * **Running**. Plan is being executed.
	* **Active**. This is monitoring plan and this plan has been sucessfully activated. Now it is waiting for trigger event to start execution.
    * **Deleted**. Plan was deleted. In Fr8, when user deletes the plan its content stays in the DB untouched. The only thing changes is the plan state. Fr8 marks such plan as deleted and stops displaying and processing it. 

### Visibility

This property is optional and consist of object with two boolean properties: **hidden** and **public**. Default is: 
```javascript
{
	"hidden": false, 
	"public": false
} 
```

* **hidden** corresponds to Plan's visibility for user: 
   * **false** is “Standard” visibility. Plan is shown in the UI.
   * **true** is  “Internal” visibility. Such plans are not generally displayed to their owning users. An example would be the Plan that registers for and records DocuSign Events when a Fr8 User has linked in a DocuSign account
* **public** shows whether Plan is published in **PlanDirectory**. If **true** then the plan is published, othewise - **false**.  

## Subplan JSON
Describes properties of a [subplan](/Docs/ForDevelopers/Objects/Plans/Subplans.md).  
* **activities** - list of activities. 
* **id** - identifier of the subplan
* **planId** - identifier of the plan this subplan belongs to.
* **name** - name of the subplan.

## Activity JSON
Describes properties of an [activity](/Docs/ForDevelopers/Objects/Activities.md)  
* **name** - name of the activity. This property is displayed to a user
* **activityTemplate** - [Activity Template](/Docs/ForDevelopers/Objects/ActivityTemplates.md).
* **id** - Identifier of the activity
* **planId** - identifier of the plan this activitiy  belongs to. 
* **parentPlanNodeId** - identifier of the parent node this activity belongs to. This can be the Id of the subplan or Id of other activity.
* **ordering** - activities within the common parent are being displayed and executed in the order that is defined by this property
* **crateStorage** - [Crate Storage](/Docs/ForDevelopers/Objects/DataTransfer/CrateStorageDTO.md) where activity configuration is stored.
* **authTokenId** - identifier of the authentication token
* **childrenActivities** - list of children activities. 

[Go to Contents](/Docs/Home.md)  
