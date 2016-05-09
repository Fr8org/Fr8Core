# ACTIVITY DEVELOPMENT GUIDE
[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md) 
 
## General Information

[Activities](https://github.com/Fr8org/Fr8Core/blob/master/ForDevelopers/Objects/Activities.md)

A well designed Fr8 Action encapsulates a useful piece of computational functionality and wraps it in a friendly point-and-click interface. It looks “upstream” to dynamically discover what other Actions, Crates, Fields and Lists have been inserted by the builder of a Route, so that it can offer up ways for the User to tie the data together.

[Tutorials: A Basic Twilio Terminal That Can Send an SMS Message](https://github.com/Fr8org/Fr8Core/blob/master/ForDevelopers/Tutorials/TwilioTutorial.md)

### Action Testing

Each Action needs to have integration tests that test calls to /configure (both initial and followup configuration response), /activate, and /execute.

### Solutions

Solutions are specialized Actions that encapsulate a series of child actions in a unified look and feel.

Tutorials: Solution Development Guide

[Building Documentation for an Activity](https://github.com/Fr8org/Fr8Core/blob/master/ForDevelopers/ActivityDevelopmentBuildingDocumentation.md)

[Activities – Communication](https://github.com/Fr8org/Fr8Core/blob/master/ForDevelopers/ActivitiesCommunication.md)

### Before You Begin

[Plans](https://github.com/Fr8org/Fr8Core/blob/master/ForDevelopers/Objects/Plans.md)

[Modified Action Controller in Terminals](https://maginot.atlassian.net/wiki/display/DDW/Modified+Action+Controller+in+Terminals)

It’s very important that you understand the role of Crates and how Actions pull data from both upstream and downstream sources.

## Manifest Schemas

The heart of a Crate is its contents, and we’re increasingly defining and managing those contents by creating DTO-like objects called [Manifest Schemas](https://maginot.atlassian.net/wiki/display/SH/Defined+Crate+Manifests). When packing a Crate, you should be able to instantiate a ManifestSchema, fill it with data, and then just serialize it with one command.

## Working with Crates

Methods that return a CrateDTO should generally be named “PackCrate_[crate type]”

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md) 
