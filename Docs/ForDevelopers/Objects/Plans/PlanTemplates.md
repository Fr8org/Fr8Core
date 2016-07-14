# PLAN TEMPLATES
[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  

Plan templates allow users to save an existing plan to a template and to create a plan from a template. 
A user can share a template with other users.
Changes to plans and activities don't modify the template, once a plan template is saved.
Plan templates hold the state of activities and don't store authentication tokens.

Plan template consists of Plan Node Descriptions, Activity Descriptions and Node Transitions 
```
            "id" : "1"
            "name": "testplan",
            "startingPlanNodeDescriptionId": 20,
            "planNodeDescriptions": {...}
            "description":"test_description"
```

Plan Node Description structure:

```
            "id": 20,
            "name": "Add Payload Manually",
            "parentNodeId": 0,
            "transitions": {...},
            "activityDescription": {...},
            "subPlanName": "Starting Subplan",
            "subPlanOriginalId": "94135bce-3c9e-4190-b865-698e1dd1f726",
            "isStartingSubplan": true
```

Plan node descriptions and activity descriptions have a one-to-one relationship

Activity Description structure:
```
                "id": 25,
                "name": "Add Payload Manually",
                "version": "1",
                "originalId": "55d586f5-773b-4b73-94a4-d3f6d4b832fe",
                "crateStorage": {...},
                "activityTemplateId": "43652560-e432-479b-ac6e-6e952e6eaf6a"
```

Node Transtitions structure
```
                    "id": 20,
                    "transition": "jump",
                    "activityDescriptionId": 24,
                    "planTemplateId": null,
                    "planId": null
```

Transitions can have a "transition" property set to "Downstream", "Child", "Jump"
One Node Description can have many transitions, but only one of each "transition" type.
Activity Description stores activity's Crate Storage unencrypted.
`originalId` and `subPlanOriginalId` properties are needed only for correct processing of Fr8Core "Test And Branch" activity. 
The only activity at this moment, that can perform "jumps" to activities, subplans and plans at execution.
Plan templates don't store nodes relationships in a way Plans do, instead they display transitions in a generalized way.


[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  
