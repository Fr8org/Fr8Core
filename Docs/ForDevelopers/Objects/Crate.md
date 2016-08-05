# Crates

[Home](/Docs/Home.md)  

Data is transported throughout the Fr8 network  in Crates. A Crate is simply a json container for storing data. Crates are currently generated primarily for two specific purposes: 

1. When a user configures a Plan and its Activities in the Plan Builder (we call this "Design-Time"), information about the Activities and settings is stored in Crates that are then put into the CrateStorage of a Plan [Container](/Docs/ForDevelopers/Objects/Containers.md).

2. When a Plan gets run (at "Run-Time"), a Payload Container is created, and as the different Terminals carry out their Activity processing, data is generated, crated, and stored in the Payload [Container](/Docs/ForDevelopers/Objects/Containers.md).



### Structure

In the real world, a Crate consists of the stuff inside the Crate (the “Storage”), and some amount of metadata about the Crate (such as receipts, logs, manifests, and directives) that is typically found taped, stapled, nailed, or painted onto the outside of the Crate.

![crate_manifest](/Docs/img/Fr8Crates_CrateManifest.png)

Fr8 Crates are similar. Crate properties can be thought of as the "inside" of the Crate, which is stored in a JSON element called contents, and the "outside" of the Crate, which is generally referred to as the Manifest of the Crate, and which includes a bunch of individual pieces of metadata about the Crate, and (optionally) the ID of a registered ]Manifest schema](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/CratesManifest.md) (generally just called a "Manifest") that provides a description of the structure of the data in the CrateStorage.

![crate_json_example](/Docs/ForDevelopers/Objects/crate_contents_example.png)

In the above example, a fragment of the Crate JSON for a Crate of UI Controls can be seen. The default Fr8 Client knows how to process Crates of UI Controls as long as they use the schema defined on the fr8.co registry at ID=6. (For more information on the registry, click [here](/Docs/ForDevelopers/Objects/CratesManifest.md))



### Additional information
- [Crate JSON Definition](/Docs/ForDevelopers/Objects/CrateJSON.md)
- [Signalling of Crate Data](/Docs/ForDevelopers/OperatingConcepts/Signaling.md)
- [Crate Development Best Practices](/Docs/ForDevelopers/DevelopmentGuides/CrateBestPractices.md)


[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  
