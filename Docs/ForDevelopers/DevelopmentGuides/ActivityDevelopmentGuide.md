# ACTIVITY DEVELOPMENT GUIDE
[Go to Contents](/Docs/Home.md)


Here's what you need to know in order to build a new Activity for an existing Terminal, or modify an existing Terminal.

## Before You Start
Before proceeding, you really want to understand how Activities work:
*  [Fr8 Activities](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Activities.md)

You should also be familiar with the following core topics:

*  [Fr8 Architecture](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/ArchitecturalModel.md)
*  [Fr8 Events](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/Events.md)
*  [Fr8 Crates](/Docs/ForDevelopers/Objects/Crate.md)
*  [Fr8 Containers](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Containers.md)

These topics are more critical to Terminal developers, but are still nice to know about:
*  [Fr8 Plans](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Plans.md)
*  [Fr8 Authorization](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Services/Authorization.md)

A well designed Fr8 Activity encapsulates a useful piece of computational functionality and wraps it in a friendly point-and-click interface. It looks “upstream” to dynamically discover what Crates and Fields are being signalled by upstream Activities, so that it can offer up ways for the User to tie the data together.

## High Level Activity Design
An Activity needs to respond effectively to two core requests: Configure and Run, and important auxiliary requests of Activate, Deactivate, Documentation.

### Designing your Configure functionality.
Learn the basics of [how Configuration works](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/ActivityConfiguration.md)

The Configure phase happens at design-time, when a user is tinkering with the Plan Builder and crafting a Plan. During this phase, an Activity generally wants to accomplish three things:

1. [Specify the configuration UI](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/ConfigurationControls.md) that the user should be presented with, so the user can provide configuration input


2. In certain circumstances, update that UI by using the initial user input to retrieve additional data from the backend web service. Example: a Query Salesforce Object activity initially can only request that the user specify which object they're interested in. Once the user does that, though, say by choosing "Lead", the activity can provide Lead-specific object fields as additional configuration UI

3. Generate and add data to the Activity JSON that will serve to signal to downstream activities what data structures will be available at runtime. (See [Signalling](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/CrateSignalling.md)) 

Keep in mind that you should package all of the data you generate during the configuration process into [JSON crates](/Docs/ForDevelopers/Objects/Crate.md) and add those crates to the Activity JSON element. For most activities, though, there's only likely to be one crate you need to think about: your crate of UI Controls. 


Best Practices

1) Make use of Followup Configuration. 

This has two parts. First, insert [Control Configuration Events](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/ConfigurationControlEvents.md) into your UI specifications with "requestConfig" actions, so that the client will know when to kick off a new conifgure call to you. Secondly, build followup configuration logic that leverages initial user inputs to provide them with progressively richer, more specific UI.

2) Take advantage of the growing set of [Configuration
Controls](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/ConfigurationControls.md)

It's pretty obvious how to get benefit by calling for a checkbox or text field. But also be aware of some of the richer compound UI elements that can be invoked with a single line of JSON, such as TextSource, which enables the user to provide a specific input or map an upstream data property.

3) Take advantage of SDK utility functions

The amount of SDK support will always vary from platform to platform, but in many cases, there are helper methods for generating the appropriate JSON for a particular piece of UI, that simplify your code. For example, the .NET Fr8 tools provide POCO objects for invoking UI, and no JSON manipulation is required: http://screencast.com/t/dOjZ7ykXXV

4) Copy, Copy, Copy

Look for existing Activities that carry out similar UI configuration or config data processing, and copy their code. No need to reinvent the wheel. (of course, this is much more practical if you stick to activities coded in your dev language of choice)

### Working with Crate Manifests
You can design your activity to process a single object. We recommend that you try and identify and work with existing manifests, if they meet your needs. Manifests usually are designed to represent 1..N objects. You should process the Manifest if it has a single object and be prepared for users to make use of the Loop activity, which will create a crate with a manifest and a single object. If you're handling a Manifest that can work with multiple objects and your activity can only process a single object, return an error message."

### Designing your Run functionality.

First, make sure you're familiar with the general mechanisms of [Plan Running and Activation](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/PlansActivationAndRunning.md).

It's important to make sure that the data structures you create at run-time match the signals you published at design time. In other words, if you promise downstream activities that you're going to generate at run-time a field called Query Count, make sure you actually do so. 

When you return control to the Hub, you not only pass back the modified Payload Container, but also generate an [ActivityResponse](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Activities/ActivityResponses.md). This is a very powerful mechanism, allowing an Activity to request significant changes to the Plan's overall control flow. One example: An Activity can basically instruct the Hub to suspend processing of the Plan until such a time as the Activity's Terminal generates a Resume event and posts it to the Hub. This is useful if the Activity needs to wait for something that might take a long time, like waiting for a person to respond to some sort of message. ActivityReponse is also the mechanism used to convey error information to the user, and providing good, useful error information is critical to making Plan design and operation straightforward.

### Designing your Activation and Deactivation functionality

Activation is a phase that takes place before a Plan is run. It is primarily used for [Validation](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/ActivitiesValidation.md) and for registering for notifications with an external web service. See [Plan Activation](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/PlansActivationAndRunning.md) for more information.

When a Deactivation call is received, an activity should  deregister from an external event system. Please see [Monitor Activities](/Docs/MonitorActivities.md). This call must return an ActivityDTO as a response.

### Documentation
Activities receive calls at the [/documentation endpoint](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/ActivityDevelopmentBuildingDocumentation.md) when the client is requesting on behalf of the user some chunk of help text or or other documentation.  

It's a Best Practice to provide rich support for documentation requests.


[Tutorials: A Basic Twilio Terminal That Can Send an SMS Message](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Tutorials/TwilioTutorial.md)


## Advanced Topics


[Solutions](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/Solutions) are Activities that generally generate 1 or more child and sibling activities programmatically. They're designed to simplify the process of building Plans.


[Platform SDKs (.NET, Java, Ruby, Python](https://github.com/Fr8org/Fr8Core/blob/docs5/Docs/ForDevelopers/SDKHome.md)
