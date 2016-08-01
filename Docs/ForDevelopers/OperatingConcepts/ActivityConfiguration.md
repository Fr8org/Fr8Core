# ACTIVITY CONFIGURATION

[Go to Contents](/Docs/Home.md)  

Activities are configured at [Design-Time](/Docs/ForDevelopers/OperatingConcepts/Fr8Modes.md). Configuration is the responsibility of the Terminal that hosts that Activity. The Terminal instructs the Client as to what UI controls should be displayed and as a result the Client doesn’t need to know anything  about the Activity. Likewise the Hub stays uninvolved with Activity Configuration, serving as a simple conduit to enable the Client and Terminal to exchange data.  

Example: The Write To Azure Sql Server action needs a connection string in order to work. It defines that need in json and passes the json back in response to Configure requests. The client front-end will likely render UI based on this json, showing the user something like this:  

![Write To SQL Server](/Docs/img/ActivityConfiguration_WriteToSQLServer.png)

This approach has a big benefit: a developer can create a new Activity, complete with sophisticated configuration UI, and deploy it without any changes being made to the Hub or Client code. It’s analogous to being able to deploy a web page without having to change any of the web servers or browsers “out there” on the web, so long as you stick to HTML and Javascript standards.  

The complexity of the configuration process depends entirely on the Activity. Some Activities require no configuration at all, while others require multiple steps.  

Key Design Concepts
====================

## Configuration involves the manipulation of a JSON structure called an Activity Container that contains all information about the Activity. 
When the Activity is created, this Activity Container JSON is generated between the client and hub. When a configure call is made to a Terminal, the Activity Container is passed to the code responsible for activity configuration. Most Terminals will make modifications to the Activity Container, generally by adding or modifying JSON data elements called Crates.  The most common modification a Terminal typically makes to an Activity Container is the creation or updating of a Crate of UI Controls which will be used by the client to show the user configuration UI. Once the terminal is completed with a round of configuration, it returns the modified Activity Container back to the Hub, which saves changes before passing it on to the client.

##  Activities define their UI via a JSON mechanism  

Configuration begins when a Client adds or loads an Activity into a Plan. the Client POSTs to the Hub’s /activities/configure endpoint. The Hub checks to see if the Activity [requires authorization](/Docs/ForDevelopers/Services/Authorization.md), which basically means that its Terminal is going to need to log into a web service. If authorization is required, the Hub checks to see if it is storing a relevant AuthorizationToken. If so, the token is POSTed to the appropriate Terminal’s /actions/configure endpoint along with the Activity json. (If a token is not available, the Hub responds to the Client, instructing it to initiate a (usually OAuth) authorization process).  

Fr8 breaks configuration calls into two buckets: Initial and Followup. The Activity code evaluates the received request and decides whether the call is an Initial or Followup call, and acts accordingly.  The typical usage pattern is to respond to Initial calls by composing the configuration UI that the user should see, and respond to Followup calls by updating the configuration UI based on the user’s choices. For example, the Monitor DocuSign Activity’s Initial Configuration creates a drop down list box populated with the names of all of the user’s DocuSign templates. When the user selects a template, a followup call is made to the Activity, which extracts the custom fields from that particular template and adds them to the Activity. This enables downstream actions to use those custom fields in their own configuration (“Extract the value of the custom field called Doctor and add it to this document”)    

### Triggering Configuration Requests using Events  

There are a couple of other mechanisms that an Activity can use to trigger configure calls back to itself. All of the Configuration Controls can have an Events property, and an Activity can set a control to trigger a new configuration call back to itself. For example, in the above example, the Followup Configuration call is created when the Activity adds an Event property to the Select Template drop down list box called onSelect which instructs the Client to RequestConfig.  

Through this process, an iterative sequence can be created where the Configuration UI that the user sees keeps updating in response to the user’s selections.  

## Triggering Reconfiguration  

When the client receives a response to a Configuration call, it triggers configuration calls for all downstream activities. This ensures that downstream activities aren't left referencing now-invalid upstream signals. On the Roadmap, we intend to improve the efficiency of this process.

![Configure Flow](/Docs/img/ActivityConfiguration_ConfigureFlow.png)

## Activity Configuration Can Use a Set of Defined UI Controls

Learn more about the [UI Control Definitions.](/Docs/ForDevelopers/DevelopmentGuides/ConfigurationControls.md)

## Activities Signal To Downstream Siblings
Signaling is the process by which an Activity informs other Activities what data it will be able to produce at run-time. This is critical to enabling easy design-time configuration. [Learn more](/Docs/ForDevelopers/OperatingConcepts/Signaling.md).

=======================
this content has some overlap with the above content and should be merged into it.

###  

When user creates an activity on PlanBuilder (PB), hub calls configure function of the activity. (Please see [Hub-Terminal Communication](/Docs/HubTerminalCommunication.md)) An activity must respond this call with an ActivityDTO.
Respond of the activity gets sent to PB. All activities are expected to have a [StandardConfigurationControlsCM](/Docs/Manifests/StandardConfigurationControlsCM.md) with a label of "Collection_Controls". This is the UI part of an activity. PB inspects this crate and renders each ControlDefinitionDTO sequentially (Read about [Activity Controls](/Docs/ActivityControls.md)). And voila our activity has a face which can be configured by the user.

Configure calls allow user to design and configure activity for run and activation time.

When user first creates an activity, hub calls activity configure function with an empty storage. When a configure call contains an empty storage, activity might assume that this is the initial call. Activities are expected to create the StandardConfigurationControlsCM and insert it into their storage on their initial call. An activity might need to do some additional operations too, like connecting to a remote service and loading data. (PostToSlack activity loads channel list from slack and inserts this data into a dropdown control. allowing user to select a channel)
When a configure call contains non empty storage, activity might assume this is the followup call. this call might be triggered by a ControlDefinitionDTO change or manually by user. An activity might need to do operations on each configure call. Like checking if the user filled a required data to connect to an external service.

Each configure call to activity re-renders activity UI completely. An activity might add new controls or modify existing ones on these calls.

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  
