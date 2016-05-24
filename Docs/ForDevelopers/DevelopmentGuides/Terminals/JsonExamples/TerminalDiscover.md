```json
`{
  "crateManifestType": {
    "type": "Standard Fr8 Terminal",
    "id": 23
  },
  "definition": {
    "name": "terminalGithub",
    "label": "GitHub",
    "version": "1",
    "terminalStatus": 0,
    "endpoint": "http:\/\/localhost:9000",
    "description": "GitHub Terminal which monitors commits to a repository",
    "authenticationType": 3
  },
  "activities": [
    {
      "id": null,
      "name": "GitHub Subscribe",
      "label": "Subscribe to GitHub Repository",
      "version": "1",
      "webService": {
        "name": "GitHub",
        "iconPath": "https:\/\/assets-cdn.github.com\/favicon.ico",
        "id": 0
      },
      "terminal": {
        "name": "terminalGithub",
        "label": "GitHub",
        "version": "1",
        "terminalStatus": 0,
        "endpoint": "http:\/\/localhost:9000",
        "description": "GitHub Terminal which monitors commits to a repository",
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
      "name": "GitHub Pull",
      "label": "Pull from GitHub Repository",
      "version": "1",
      "webService": {
        "name": "GitHub",
        "iconPath": "https:\/\/assets-cdn.github.com\/favicon.ico",
        "id": 0
      },
      "terminal": {
        "name": "terminalGithub",
        "label": "GitHub",
        "version": "1",
        "terminalStatus": 0,
        "endpoint": "http:\/\/localhost:9000",
        "description": "GitHub Terminal which monitors commits to a repository",
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