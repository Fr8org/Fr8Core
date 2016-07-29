# Manifests

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  
A Manifest is a simple json schema that defines the contents of a Crate of data. Use of Manifests is optional. They’re essentially a way for activities to effectively process structured data. Put another way, if you know the Manifest of a Crate, you can use it to deserialize the contents (which is just a big json string) into structured data. This can be as basic as a set of key/value pairs, but the real power of manifests comes when unrelated activities both process something much more structured, like a DocuSign Envelope. 

While any builder of Fr8 Actions can define private Manifests for their own purposes, the bulk of the value of Manifests comes through a shared registry. For example, in the [Fr8 Manifest Registry](https://fr8.co/dashboard/manifest_registry), Id 14 is assigned to a Manifest called DocuSignEnvelope that looks like this:

![manifest_docusign_envelopes](https://github.com/Fr8org/Fr8Core/blob/master/Docs/img/CratesManifest_ManifestDocusignEnvelopes.png) 

The Fr8 Company maintains a public registry of Manifests at fr8.co/dashboard/manifest_registry. Anyone can register a new Manifest, although registration of manifests that are equivalent or highly similar to existing manifests is discouraged. The registry generates a unique id that can be used in Crates.

An Action designer can design an Action that works with DocuSignEnvelopes. They can assume that inbound crates that contain DocuSignEnvelope data will deserialize into this object.

Manifests are versioned, and will get generally get richer and more comprehensive over time.



### Special Manifests

The Baseline Manifests: Standard Payload Data and Standard Configuration Fields

If there isn’t a more targeted manifest that fits, these are the fallback. Both essentially are a simple List of JSON objects, which can have arbitrarily complex values.

### Developing Manifests

See the [Manifest Development Guide](https://github.com/Fr8org/Fr8Core/blob/docs5/Docs/ForDevelopers/DevelopmentGuides/ManifestDevelopmentGuide.md)

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md) 
