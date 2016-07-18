Manifest Development Guide
=========================

The biggest historical limitation, when it comes to building powerful IPaaS solutions, has been figuring out a way to work with structured data. Fr8 addresses 
that via [Crate Manifests](https://github.com/Fr8org/Fr8Core/blob/docs5/Docs/ForDevelopers/Objects/CratesManifest.md).


4. Consider Manifest Enumerability
---------------------------------
By convention, Fr8 recommends that Manifests be constructed as a collection (array) of JSON elements, as opposed to a single JSON element.
This makes the crate data enumerable and enables downstream activities to iterate through it, either explicitly 
(by processing each element in the collection themselves) or implicitly (relying on the Loop Activity to serve one element 
at a time to its child Activities). Keep this in mind as you 
