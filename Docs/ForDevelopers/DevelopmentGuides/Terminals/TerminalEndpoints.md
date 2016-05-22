# Terminal Endpoints

Terminals reply to requests on specific endpoints. The replies are always in JSON and the structure of that JSON varies by endpoint.

The table below lists the endpoints on which terminals are expected to reply with sample JSON.

| HTTP Method | Endpoint | JSON |
|:-----------:|:--------:|:----:|
|GET | /terminals/discover | [Example](JsonExamples/TerminalDiscover.md)|
| POST | /terminals/activity/&lt;activity-name&gt; | [Example](JsonExamples/TerminalActivity.md)|
| POST | /authentication/internal | [Example](JsonExamples/TerminalAuthInternal.md)|
| POST | /authentication/initial_url | [Example](JsonExamples/TerminalAuthInitialUrl.md)|
| POST | /authentication/token | [Example](JsonExamples/TerminalAuthToken.md)|
| POST | /activities/configure | [Example](JsonExamples/TerminalActivitiesConfigure.md)|
