# CRATE DEVELOPMENT GUIDE
[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md) 

Crates are at the core of Fr8. They are json data containers.

Crates are used in two different modes of operation. During Plan Design, Actions use crates to store user-provided input and the result of computations of data from upstream.  During Plan Execution, the actual payload data is stored in crates in the Container that is routed from Activity to Activity.

### Field Availability

Good Actitivy design requires designers to try and filter out unnecessary noise data when compiling lists from which the user can choose. The Availability property helps with this.

Most fields are used in either Plan Design or Plan Execution, but not both. For example, suppose you have the following Plan Design:

1. Monitor DocuSign for newly signed Envelopes created using a specific DocuSign Template.
2. When one is found,  send an SMS message using Twilio to a specified phone number.
The user creating this Plan needs to be able to choose from a list of their DocuSign Templates. This list of names should be available during Plan Design for them and isn’t relevant during Plan Execution (only the selected name is relevant at that point). On the other hand, at Plan Execution, the Envelope Id is critically important, but it isn’t available at all during Plan Design.

The Availability property of a Field provides a way to conveniently filter out irrelevant data from the lists that users are shown. If you mark a field like EnvelopeId as “Run-Time”, the field will be filtered out of list boxes that are only relevant during Plan Design. If you mark the field “”Configuration”, then the field will be filtered out of lists that are intended to show only run-time data.

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md) 
