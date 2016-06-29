# ACTIVITY DEVELOPMENT GUIDE
[Go to Contents](/Docs/Home.md)

## General Information

As a starting point, read about [Activities](/Docs/ForDevelopers/Objects/Activities.md) here


A well designed Fr8 Activity encapsulates a useful piece of computational functionality and wraps it in a friendly point-and-click interface. It looks “upstream” to dynamically discover what Crates and Fields are being signalled by upstream Activities, so that it can offer up ways for the User to tie the data together.

To understand activity execution logic, please first read terminal development documentations. Those documentations will help you learn about our main data structures; which are Fr8DataDTO, ActivityDTO and ActivityTemplateDTO. [Terminal Development](/Docs/TerminalDevelopment.md), [Data Structures](/Docs/DataStructures.md)


### Before You Begin

The heart of an activity is its [CrateStorage](/Docs/CrateStorage.md) and its contents. Activities carry their data with their crateStorage. We’re increasingly defining and managing those contents by creating DTO-like objects called [Manifest Schemas](https://maginot.atlassian.net/wiki/display/SH/Defined+Crate+Manifests). Please read about manifest schemas to better understand activity development. When packing a Crate, you should be able to instantiate a ManifestSchema, fill it with data, push that crate to storage and then just serialize it with one command.

## Activity Functions (Interface)

An activity must have 5 functions. Hub might call any of these functions during design time, run time or for documentation of an activity.
These functions are "Configure, Run, Activate, Deactivate, Documentation"

### 1. Configure

When user creates an activity on PlanBuilder (PB), hub calls configure function of the activity. (Please see [Hub-Terminal Communication](/Docs/HubTerminalCommunication.md)) An activity must respond this call with an ActivityDTO.
Respond of the activity gets sent to PB. All activities are expected to have a [StandardConfigurationControlsCM](/Docs/Manifests/StandardConfigurationControlsCM.md) with a label of "Collection_Controls". This is the UI part of an activity. PB inspects this crate and renders each ControlDefinitionDTO sequentially (Please read about [Activity Controls](/Docs/ActivityControls.md)). And voila our activity has a face which can be configured by the user.

#### 1.1. Initial and FollowUp Configuration

When user first creates an activity, hub calls activity configure function with an empty storage. When a configure call contains an empty storage, activity might assume that this is the initial call. Activities are expected to create the StandardConfigurationControlsCM and insert it into their storage on their initial call. An activity might need to do some additional operations too, like connecting to a remote service and loading data. (PostToSlack activity loads channel list from slack and inserts this data into a dropdown control. allowing user to select a channel)
When a configure call contains non empty storage, activity might assume this is the followup call. this call might be triggered by a ControlDefinitionDTO change or manually by user. An activity might need to do operations on each configure call. Like checking if the user filled a required data to connect to an external service.

Each configure call to activity re-renders activity UI completely. An activity might add new controls or modify existing ones on these calls.

Configure calls allow user to design and configure activity for run and activation time.

#### 1.2. Crate and Field Signalling

The magic of Fr8 comes from glueing non related activities together. This magic happens with crate signalling. With crate signalling we let downstream activities about which data we will make available during run time. Please read about [CrateSignalling](/Docs/Activity/CrateSignalling.md)

### 2. Run

When a plan is run, hub calls run function in each activity in order. Children activities are run before sibling activities. Activities are expected to return [PayloadDTO](/Docs/DataStructures/PayloadDTO.md) as response to a run call. An activity must get current payload by making a request to hub.

 After getting current container payload an activity is ready to run. Usually activities read UI crate and do operations according to user configurations. For example PostToSlack activity gets user selected channel and posts information to that channel on run time.

 Activity configurations might require data from upstream activities. We assume all upstream activities inserted their data to payload - we can search payload for data we are looking for during runtime. Since upstream activities might have promised us for some data during runtime.

 If we promised some data during our configuration using signalling we must insert that data to payload. So that downstream activities that depend on that data can use it during run-time.

A run call might be triggered manually by a user or by an external service. Please see [Monitor Activities](/Docs/MonitorActivities.md) to understand external events.

#### 2.1. ExecuteChildrenActivity

Hub might call run function with 2 different [Scopes](/Docs/RunScopes.md). Those are Run and ExecuteChildrenActivity. When hub decides to run an activity it calls run function with run scope. After executing our activity, if that activity has children hub moves to children activities. and after completing processing children, hub calls our activity's run function with ExecuteChildrenActivity scope. Which means "hey i just completed running your children. it is time if you want to do something about it"

### 4. Activate

On this call hub expects an activity to register to an external event system. Please [Monitor Activities](/Docs/MonitorActivities.md). This call must return an ActivityDTO as a response.

### 5. Deactivate

On this call hub expects an activity to deregister from an external event system. Please see [Monitor Activities](/Docs/MonitorActivities.md). This call must return an ActivityDTO as a response.

### 6. Documentation

This call is made to activity to show documentation. Activity must return [DocumentationResponseDTO](/Docs/DataStructures/DocumentationResponseDTO.md) as a result.


[Tutorials: A Basic Twilio Terminal That Can Send an SMS Message](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Tutorials/TwilioTutorial.md)


### Additional Topics

[Solutions](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/Solutions) are specialized Actions that encapsulate a series of child actions in a unified look and feel.

[Building Documentation](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/ActivityDevelopmentBuildingDocumentation.md)
