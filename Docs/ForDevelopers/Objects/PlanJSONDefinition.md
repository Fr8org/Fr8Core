# PLAN – JSON DEFINITION
[Go to Contents](https://github.com/Fr8org/Fr8Core.NET/blob/master/Docs/Home.md)  

The JSON specification for a Plan is:
```javascript
{
    "subroutes": [
        {
            "activities": [
                {}, 
                {}
            ], 
            "id": "e9e89349-3871-46e4-ad11-71e9e207a9f3", 
            "planId": "91d3d82e-f471-4095-bd59-9f88ea86d5af", 
            "name": null, 
            "transitionKey": null
        }
    ], 
    "fr8UserId": "fcc7d694-4820-4d04-bc83-67251fcf8a15", 
    "id": "91d3d82e-f471-4095-bd59-9f88ea86d5af", 
    "name": "MonitorAllDocuSignEvents", 
    "tag": "docusign-auto-monitor-plan-fcc7d694-4820-4d04-bc83-67251fcf8a15", 
    "visibility": "standard"
    "description": "MonitorAllDocuSignEvents", 
    "lastUpdated": "0001-01-01T00:00:00+00:00", 
    "routeState": 2, 
    "startingSubrouteId": "e9e89349-3871-46e4-ad11-71e9e207a9f3"
}
```
## Visibility

This property is optional and can have two values “standard” or “internal”. Default is “Standard”. Plans with “Internal” visibility are not generally displayed to their owning users. An example would be the Plan that registers for and records DocuSign Events when a Fr8 User has linked in a DocuSign account.

[Go to Contents](https://github.com/Fr8org/Fr8Core.NET/blob/master/Docs/Home.md)  
