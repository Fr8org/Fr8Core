Manifest Development Guide
=========================

The biggest historical limitation, when it comes to building powerful IPaaS solutions, has been figuring out a way to work with structured data. Fr8 addresses 
that via [Crate Manifests](https://github.com/Fr8org/Fr8Core/blob/docs5/Docs/ForDevelopers/Objects/CratesManifest.md).

As a Developer extending Fr8, you should try to use the existing Fr8 Manifest schemae, as defined at the Fr8 [Manifest Registry](https://fr8.co/dashboard/manifest_registry), and you should also feel free to extend existing Manifests and define new ones. Extending existing Manifests makes sense when you want to add richness or complexity to an existing definition. If your extension represents a pure subset of an existing Manifest, you may want to use the same name and increment the version number. This will allow Fr8 to continue to support the older version. 

Know the Core Manifests
--------------------------
A small set of Manifests are responsible for a lot of the data moved between Activities:

*Standard Payload Data*

This is the lowest common denominator of data interchange. It simply consists of a collection/array of N JSON elements. The elements don't have to be identical and they can be simple. 

*Standard Table Data*

This is used by Activities that can create or process tabular data. It consists of 1..N TableRow elements. The first row can be marked to be a row of headers.


Consider Manifest Enumerability
---------------------------------
By convention, Fr8 recommends that Manifests be constructed as a collection (array) of JSON elements, as opposed to a single JSON element.
This makes the crate data enumerable and enables downstream activities to iterate through it, either explicitly 
(by processing each element in the collection themselves) or implicitly (relying on the Loop Activity to serve one element 
at a time to its child Activities). Keep this in mind as you 
