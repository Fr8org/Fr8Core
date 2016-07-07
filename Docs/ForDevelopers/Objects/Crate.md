# Crates

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  

Data is transported throughout the Fr8 network  in Crates. A Crate is simply a json container for storing data. Crates are currently generated primarily for two specific purposes: 

1. When a user configures a Plan and its Activities in the Plan Builder (we call this "Design-Time"), information about the Activities and settings is stored in Crates that are then put into the CrateStorage of a Plan [Container](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Containers.md).

2. When a user runs a Plan, a Payload Container is created, and as the different Terminals carry out their Activity processing, data is generated, crated, and stored in the Payload [Container](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Containers.md).



### Structure

In the real world, a Crate consists of the stuff inside the Crate (the “Storage”), and some amount of metadata about the Crate (such as receipts, logs, manifests, and directives) that is typically found taped, stapled, nailed, or painted onto the outside of the Crate.

![crate_manifest](https://github.com/Fr8org/Fr8Core/blob/master/Docs/img/Fr8Crates_CrateManifest.png)

Fr8 Crates are similar. Crate properties can be thought of as the "inside" of the Crate, which is stored in a JSON element called CrateStorage, and the "outside" of the Crate, which is generally referred to as the Manifest of the Crate, and which includes a bunch of individual pieces of metadata about the Crate, and (optionally) the ID of a registered ]Manifest schema](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/CratesManifest.md) (generally just called a "Manifest") that provides a description of the structure of the data in the CrateStorage.




Organizationally, when constructing a Crate of data, expect that the metadata about the Crate will remain publicly viewable. CrateStorage is not automatically encrypted, but the Fr8 design anticipates that some users will want to encrypt their CrateStorage.



### Additional information
- [Crate JSON Definition](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/CrateJSON.md)
- [Signalling of Crate Data](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/Signalling.md)
- [Crate Best Practices](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/CrateBestPractices.md)


[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  
