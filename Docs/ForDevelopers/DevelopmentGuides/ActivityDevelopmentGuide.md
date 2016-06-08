# ACTIVITY DEVELOPMENT GUIDE
[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md) 
 
## General Information

As a starting point, read about Activities here:
[Activities](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Activities.md)

A well designed Fr8 Activity encapsulates a useful piece of computational functionality and wraps it in a friendly point-and-click interface. It looks “upstream” to dynamically discover what Crates and Fields are being signalled by upstream Activities, so that it can offer up ways for the User to tie the data together.

Important concept here is the interaction between activities for managing data into plan run-time. Fr8 provides a robust and loosely-coupled process for communication with upstream and downstream activities using [Activities Signalling](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Activities/Signalling.md) and working with available [Crate Data](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Activities/Signalling.md).

[Tutorials: A Basic Twilio Terminal That Can Send an SMS Message](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Tutorials/TwilioTutorial.md)

### Activity Testing

Each Activity needs to have integration tests that test calls to /configure (both initial and followup configuration response), /activate, and /execute.

### Additional Topics

[Solutions](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/Solutions) are specialized Actions that encapsulate a series of child actions in a unified look and feel.

[Building Documentation](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/ActivityDevelopmentBuildingDocumentation.md)



### Before You Begin



[Modified Activity Controller in Terminals](https://maginot.atlassian.net/wiki/display/DDW/Modified+Action+Controller+in+Terminals)



## Manifest Schemas

The heart of a Crate is its contents, and we’re increasingly defining and managing those contents by creating DTO-like objects called [Manifest Schemas](https://maginot.atlassian.net/wiki/display/SH/Defined+Crate+Manifests). When packing a Crate, you should be able to instantiate a ManifestSchema, fill it with data, and then just serialize it with one command.

## Working with Crates

Methods that return a CrateDTO should generally be named “PackCrate_[crate type]”

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md) 
