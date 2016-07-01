# CRATES

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  

Data is transported throughout the Fr8 network  in Crates. A Crate is simply a json container for storing data. Crates are used for two different kinds of data:

1. Run-Time data (or “Payload”) represents real run-time data that is being processed. Run-time data is either imported into the system (e.g. accessing a Google Spreadsheet), or generated as output by an Action.
2. Configuration data (sometimes referred to as “Design-Time” data), which is metadata used by Actions and Terminals to communicate useful information to other parts of the system. One of the most important types of these “Configuration Crates” are the UI crates that define the user interface controls that an Action needs clients to display.

At Run-Time, Crates are created into a [Container](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Containers.md), and moved from Terminal to Terminal so the Terminals can carry out the Activities of the Plan.

### Structure

In the real world, a Crate consists of the stuff inside the Crate (the “Storage”), and some amount of metadata about the Crate (such as receipts, logs, manifests, and directives) that is typically found taped, stapled, nailed, or painted onto the outside of the Crate.

![crate_manifest](https://github.com/Fr8org/Fr8Core/blob/master/Docs/img/Fr8Crates_CrateManifest.png)

Fr8 Crates are similar. Each Crate consists of a set of JSON properties. One property, CrateStorage, contains the Crate’s contents. The other properties, seen below, represent the publicly visible portion of the Crate.

Organizationally, when constructing a Crate of data, expect that the metadata about the Crate will remain publicly viewable. CrateStorage is not automatically encrypted, but the Fr8 design anticipates that some users will want to encrypt their CrateStorage.

Developers often associate Crates with [Manifests](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/CratesManifest.md)

### Additional information
- [Crate JSON Definition](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Crate.md)

- [Property: Availability](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/CratePropertyAvailability.md)

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  
