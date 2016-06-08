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
The initial Terminals being built and hosted by The fr8 Company are .Net based Azure Web App Projects

Also everyone can run their own Hub by creating a web service that is built based on [Fr8 Hub Specification](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Specifications/Fr8HubSpecification.md).

Terminals can be written in any language, and only need to support a handful of [HTTP endpoints](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/Terminals/TerminalEndpoints.md).

Once a new Terminal is created, you need to configure the Hub with entering initial Terminal Url into the list of fr8terminals.txt. This way the Hub will be aware that a new Terminal exist on this given Url. When Hubs start, they make a /discover call to the Terminal endpoints that they know about.

So a new Terminal need to implement the /discover endpoint. The Terminal need to respond with information about Terminal main data (name of the endpoint, type of authentication this endpoint is using, etc). Also this response need to contain a list of all activity types that a Terminal will be supporting. This response need to be serialized in JSON with this specific structure  [StandardFr8TerminalCM](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/RegisteredManifests.md) that encapsulates [TerminalDTO](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/DataTransfer/TerminalDTO.md) and a list of [Activity Templates](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/ActivityTemplates.md).

Once a terminal provides a list of Activity Templates, the Hub will save those values. Those new Activity Templates will be ready to use in the process of creating Fr8 Plans with combining new activities based on templates with other ones from existing terminals and making them to be interoperable with each other.

Make sure to be well aware about activity categories, and the type of functionality they need to support. Check [Activity Template Categories](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/ActivityTemplates.md#category). Also Fr8 supports standalone Activities or complex [Solutions](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/Solutions) that creates and prefills multiple child activities based on initial user input.

Terminals can be used to develope many [Fr8 Activities](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Activities.md) that has a specified JSON structure. Building new Activity need to contain implementations for four default functionalities:
* configure
* run
* activate
* deactivate
For more details how this can be invoked check [API Endpoints]((https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/Terminals/TerminalEndpoints.md))

### Configure

Every activity is configured at [Design-Time](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/Fr8Modes.md).
Whole activity configuration is decoupled from the Hub, and the Terminal is responsible for it. In the moment of building a new plan, each Terminal is giving instructions to the Client, what to display based on UI controls that specified Activity provided.

Everything that is content specific in our activity need to be packed as JSON. For that case can be used the [Crate](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/CrateDTO.md) structure inside the Activity, where we are defining and managing contents by creating DTO-like objects called [Manifest Schemas](https://maginot.atlassian.net/wiki/display/SH/Defined+Crate+Manifests). So an Activity need to be able to pack Crates, for a specific Manifest Schema, where we fill in data, and then serialize as JSON and add that new crate to the Activity [Crate Storage](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/DataTrasfer/CrateStorageDTO.md).

For building Activity UI Controls, like dropdowns, text boxes, buttons etc, Fr8 support this given [Controls](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/ConfigurationControls.md). When the Hub process a configure request received from a terminal, it iterate through each activity Crate Storage and looks for a specified Crate manifest types. When we want to render UI Controls we need to provide in our activity JSON packed those crates with a Crate of Manifest Type: Standard UI Controls.

Configure functionality is structure from two key concepts Initial Configuration and Followup Configuration.

* On Initial Configuration in mandatory to provide Crates for Standard UI Controls (described above) and also provide Authentication logic packed as Standard Authentication Type.

When new Terminal need to provide authentication to a 3rd-party service provider (example authentication with Google), a developer need to implement /authentication endpoint, where the endpoint will provide /authentication/token request with response of [AuthorizationTokenDTO](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/DataTransfer/AuthorizationTokenDTO.md). Fr8 supports OaAuth authentication, performed by opening external service OAuth page.
Activity that want to support authentication, need to set the needAuthentication property inside Activity Template.

* Standard UI Controls have a support for creating onChange event handlers, so when a user tries to modify some control on the UI, like select an item in a drop down list, or click a submit button in the activity, that front end action will invoke a new call to the Terminal, where some other actions can be performed based on this request. This is called a Followup configuration, where some functionality are invoked based on client interactions with Activity UI.

Important concept here is the interaction between activities for managing data into plan run-time. Fr8 provides a robust and loosely-coupled process for communication with upstream and downstream activities using [Activities Signalling](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Activities/Signalling.md) and working with available [Crate Data](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Activities/Signalling.md).

### Run

When a user click run plan, the hub goes activity by activity in order and calls each terminal /activities/run endpoint that need to be implemented. Once an Activity is run must create and pack a crate of [OperationalState](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Activities/OperationalStateCM.md) Manifest Type, that will hold information about the payload provided from running the activity, status of the activity.

Based on the configure process, in the run functionality we must provide payload values. That specific values are packed in a crate, linked to the main plan [Container]((https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Containers.md)) and transfer JSON back as [PayloadDTO](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/DataTransfer/PayloadDTO.md).

Example: Configure a Activity to Get Google spreadsheet where we select a spreadsheet from a dropdown list in Design-Time, and after when a user click run, this activity run method will read all content from spreadsheet and provide payload values.

### Activate and Deactivate

In the process of activate an Activity, we need to check if every condition is satisfied for running that activity. With simple logic we can get the configuration controls, and check if some condition is not satisfied like a value is not selected from a dropdown list. Then the functionality for activate Activity need to provide validation errors that will prevent from running whole plan and break logic.

Validation errors need to be packed in crate as [ValidationResultsCM](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/RegisteredManifests.md)) Crate Manifest Type.
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
