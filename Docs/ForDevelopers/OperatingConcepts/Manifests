Crate Manifests
==


Manifests are essentially JSON schemas that specify a set of named properties and data structures. 
For example a Manifest called StandardApples might list three string properties "Color", "Variety", and "Weight".
The purpose of Manifests is to make it possible for unrelated Activities to share and process structured data without knowing about each other. 
For example, suppose that Joe wants to build an Activity that monitors for DocuSign Envelopes that users have sent. Meanwhile, Jane has an Activity that 
archives a DocuSign Envelope to a Dropbox Folder. DocuSign Envelopes have multiple Recipients and each Recipient can have multiple Tabs and Fields.
If both Activities rely on the same Manifest, then Jane's JSON processing code will work on the data delivered from Joe's Activity, far upstream.

