# TERMINAL DEVELOPMENT GUIDE

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)

## Basic Topics

Before proceeding, make sure you're familiar with the following basic topics:
*  [Fr8 Architecture](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/ArchitecturalModel.md)
*  [Fr8 Events](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/Events.md)
*  [Fr8 Crates](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/CrateDTO.md)
*  [Fr8 Containers](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Containers.md)
*  [Fr8 Activities](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Activities.md)
*  [Fr8 Plans](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Plans.md)
*  [Fr8 Authorization](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Services/Authorization.md)

## General Information

This is platform-independent. For platform-specific information you can read on the bottom of the page.

Terminals are discrete Web Services that host Activity functionality.
The main Fr8 Hub runs many of these Terminals itself, but the architecture is decoupled: as far as the Hub is concerned, each Terminal is on the other end of an HTTP request. A fully operational Hub may work with hundreds of different Terminals.


Also you can run your own Hub by creating a web service that is built based on [Fr8 Hub Specification](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Specifications/Fr8HubSpecification.md).

Terminals can be written in any language, and only need to support a handful of [HTTP endpoints](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/Terminals/TerminalEndpoints.md).

Connecting to Hubs
===

Once a new Terminal is created, you need to bring it to the attention of one or more Hubs by requesting that the Hub operator add your Terminal's endpoint URL to the Hub's fr8terminals.txt file. When Hubs startup, they make a /discover call to each of the Terminal endpoints that they know about.

So each Terminal need to implement the /discover endpoint. When this call is received, the Terminal responds with a set of information that tells the Hub what Activities are currently being offered.  This response need to be serialized in JSON with this specific structure  [StandardFr8TerminalCM](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/RegisteredManifests.md) that encapsulates [TerminalDTO](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/DataTransfer/TerminalDTO.md) and a list of [Activity Templates](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/ActivityTemplates.md).

The ActivityTemplates are saved by the Hub, and provided to the Client so it knows what Activities it can offer to users. Users can add Activities from multiple Terminals to form a single Plan.

 Activities should be put into categories. See [Activity Template Categories](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/ActivityTemplates.md#category). Terminals can also build more complex Solutions that are composed of multiple Activities.  [Solutions](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/Solutions).

Endpoints
===

Terminals need to support the following core endpoints:
* configure
* run
* activate/deactivate
See the full set of Terminal [API Endpoints]((https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/Terminals/TerminalEndpoints.md))

### Configure

/Configure calls are received by the Terminal only during [Design-Time](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/Fr8Modes.md). User input is used to [configure Activities](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/ActivityConfiguration.md). This takes place as one or more /configure calls that originate on the Client and are handled by the Terminal's Activity code. During Configuration, the Hub passes data back and forth but generally stays out of the way.  In the moment of building a new plan, each Terminal is giving instructions to the Client, what to display based on UI controls that specified Activity provided.

The data provided to the Terminal as part of the /configure call is the [Activity JSON](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/ActivityJSONDefinition.md). When the user initially adds an Activity to their Plan, this Activity JSON is mostly empty. One of the Terminal's first jobs is to add information about the UI that the Terminals wants the User to see. When the User then manipulates the UI and provides information, that information is stored in the Activity as well. There can be multiple rounds of /configure round trips, as the user adds or modifies settings, and the Terminal responds by providing more or different information in return. 

Some elements of the Activity JSON are hard coded, but for others, Fr8 uses its standardized data normalization mechanism called [Crates](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/CrateDTO.md). Crates are also JSON objects, and an Activity can store one or more Crates in it. Crates are also used in the Containers that contain Run-Time Payload. 

Generally, there two things a Terminal does when one of its Activities receives an initial /configure call:
1) it authenticates the user's token, and accesses the user's web service account
2) it specifies the UI that it wants the user to see 

Terminal builders specify the UI they want by choosing from the set of available UI [Controls](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/ConfigurationControls.md) and specifying them in JSON in a Crate that is stored in the Activity and returned to the Hub for delivery to the Client, which will render the appropriate UI.

Note that this Crate of UI Controls, like other Crates is tagged with a specific [Manifest](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/CratesManifest.md), which specifies its contents. Manifests are collected and registered in central Manifest Registries so Activity designers can exchange structured data.

In general, the SDK for a particular platform will include helper classes and methods so that the Activity builder doesn't actually have to manipulate JSON directly. These helper classes typically deserialize the JSON into objects. 

A Terminal should always make sure that any Activity it returns to the Hub contains a Crate of Standard UI Controls, even if the Activity requires no configuration (in that case the Fr8 Convention is to state "This Activity requires no configuration" in the UI as a text block).

Authorization
===

If a Terminal is providing Activities that require authentication, it should specify that in the corresponding ActivityTemplates. This causes the Hub to look for the presence of a stored [AuthorizationToken](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/DataTransfer/AuthorizationTokenDTO.md) each time it receives a /configure call for the Terminal. If the token is present it is added to the Activity and presented to the Terminal. When a Terminal receives a token with an Activity, it should use it when it needs to access customer data. 

If the token is not present, the Hub returns a message to the Client instructing it to [start an authorization process](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Services/Authorization.md). This results in a call to the Terminal's /authentication/request_url endpoint. The Terminal should respond to this call with a URL that the client can use to initiate an OAuth session (non-O-Auth authentication is possible as well). The client redirects to that URL to start the OAuth process. Once that's done, the Terminal will receive a call from the Hub to its /authentication/token endpoint and it should then return the token to Hub  for storage with the User's account. 

For more information on Authorization and Security, see (https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Services/Authorization.md)

### Run

A Terminal will receive a /activities/run call when the Hub, while processing a Run-Time instance, reaches that particular Activity.  
The Terminal will be passed a JSON [Container](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Containers.md)
Payload data is stored inside the Container in one or more Crates. The Terminal should pass the Container to the #Run method of the corresponding Activity, modify the Container as appropriate (for example, by adding a new Crate of processed results), and return the Container with an Activity Status message. 

A Terminal may choose to store information in the Container's [OperationalState](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Activities/OperationalStateCM.md) Crate. This is useful to preserve state in scenarios where there might be loops or flow resulting in the Terminal being called again later. 

Example: Configure a Activity to Get Google spreadsheet where we select a spreadsheet from a dropdown list in Design-Time, and after when a user click run, this activity run method will read all content from spreadsheet and provide payload values.

### Activate and Deactivate

The Activation process is currently used primarily for [Validation](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/ActivitiesValidation.md). Many user inputs can be validated as part of the /configure calls, but there's no guarantee that all user inputs will trigger subsequent /configure calls. As a result, the Hub calls /terminals/activate for each Activity in a Plan before calling Run on that Plan.

Validation errors need to be packed in crate as [ValidationResultsCM](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/RegisteredManifests.md) Crate Manifest Type.
This will prevent plan from running and user will see messages on the Client.

Activities with Monitoring category once run are active the whole time, when all others are run once and then are deactivated. Deactivate is used to stop activity from execution like prevent monitoring for some event.    

### Events

Another important part of a Terminal is developing and /event endpoint. [Events](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/Events.md) are used to receive events from external providers. Their main usage is in Activities with Monitoring category.

To provide events monitoring, inside Initial Configuration process we need to provide Crate of Manifest Type [EventSubscriptions](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/RegisteredManifests.md).

In the moment when we receive an event from a external service provider to our /event endpoint, then it's important to provide in the logic, creation of crate from [Standard Event Report](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/RegisteredManifests.md is mandatory. Once we create this object and return the crate pack as JSON, the hub will catch the crate, will take the payload values provided,  and process plan execution forward.


## Platform-Specific Information
=====
*  [.NET](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/Terminals/DevGuide_DotNet.md)
*  Java
*  Ruby
