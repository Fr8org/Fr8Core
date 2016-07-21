#JSON Payloads for Configure Terminal Activity

## URL /activities/configure

##Request
```json
{
  "Label": null,
  "Name": "Activity Name",
  "activityTemplate": {
    "id": "00000000-0000-0000-0000-000000000000",
    "name": "Activity Name",
    "version": "1",
    "label": "Activity Template Name",
    "terminal": {
      "name": "terminalName",
      "label": "Terminal Label",
      "version": "1",
      "endpoint": "http:\/\/example.com:9000"
    },
    "tags": null,
    "category": "Monitors",
    "type": "Standard",
    "minPaneWidth": 380,
    "needsAuthentication": true,
    "webService": {
      "id": 17,
      "name": "Web Service Name",
      "iconPath": "https:\/\/www.example.com\/favicon.ico"
    }
  },
  "RootPlanNodeId": "00000000-0000-0000-0000-000000000001",
  "ParentPlanNodeId": "00000000-0000-0000-0000-000000000002",
  "CurrentView": null,
  "Ordering": 1,
  "Id": "00000000-0000-0000-0000-000000000003",
  "CrateStorage": {
    "crates": [

    ]
  },
  "ChildrenActivities": [

  ],
  "AuthTokenId": null,
  "AuthToken": {
    "Id": "00000000-0000-0000-0000-000000000004",
    "Token": "d2bb85051aeb50485805b6d353ea47d31514739f",
    "ExternalAccountId": "3037127",
    "ExternalAccountName": "3037127",
    "ExternalDomainId": null,
    "ExternalDomainName": null,
    "UserId": "00000000-0000-0000-0000-0000000000005",
    "ExternalStateToken": null,
    "AdditionalAttributes": null,
    "Error": null,
    "AuthCompletedNotificationRequired": false,
    "TerminalID": 0
  },
  "documentation": null
}
```

##Response
```json

```