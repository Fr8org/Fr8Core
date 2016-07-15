#JSON Payload for Discover Terminal request

##URL /terminals/discover

##Request
__N/A__

##Response
```json
{
  "crateManifestType": {
    "type": "Standard Fr8 Terminal",
    "id": 23
  },
  "definition": {
    "name": "terminalName",
    "label": "Terminal Label",
    "version": "1",
    "terminalStatus": 0,
    "endpoint": "http:\/\/exmaple.com:9000",
    "description": "Terminal Description",
    "authenticationType": 3
  },
  "activities": [
    {
      "id": null,
      "name": "First Activity",
      "label": "Activity One Label",
      "version": "1",
      "webService": {
        "name": "Web Service Name",
        "iconPath": "https:\/\/www.example.com\/favicon.ico",
        "id": 0
      },
      "terminal": {
        "name": "terminalName",
        "label": "Terminal Label",
        "version": "1",
        "terminalStatus": 0,
        "endpoint": "http:\/\/example.com:9000",
        "description": "Terminal Description",
        "authenticationType": 3
      },
      "tags": null,
      "category": "MONITORS",
      "type": "STANDARD",
      "minPaneWidth": 380,
      "needsAuthentication": true
    },
    {
      "id": null,
      "name": "Second Activity",
      "label": "Activity Two label",
      "version": "1",
      "webService": {
        "name": "Web Service Name",
        "iconPath": "https:\/\/www.example.com\/favicon.ico",
        "id": 0
      },
      "terminal": {
        "name": "terminalName",
        "label": "Terminal Label",
        "version": "1",
        "terminalStatus": 0,
        "endpoint": "http:\/\/example.com:9000",
        "description": "Terminal Description",
        "authenticationType": 3
      },
      "tags": null,
      "category": "FORWARDERS",
      "type": "STANDARD",
      "minPaneWidth": 380,
      "needsAuthentication": true
    }
  ],
  "primaryKey": null
}
```