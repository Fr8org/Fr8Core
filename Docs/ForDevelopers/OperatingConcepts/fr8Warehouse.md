The Fr8 Warehouse
=================

Fr8 Hubs provide a storage service that can be used to store arbitrary information. This is intended not a permanent repository of information a la
Dropbox or Google Drive, and more as a temporary utility, which is how warehouse at ports and docks tend to work.

The Warehouse is patterned after the multitenant architecture used by Salesforce.com and described [here](https://developer.salesforce.com/page/Multi_Tenant_Architecture).
The tables that form the Warehouse are currently part of the main Fr8 Hub SQL Database, although this may change in the future. 

Here's an example of how the Fr8 Warehouse can be used: Suppose a user builds the DocuSign Fr8 Solution "Track DocuSign Recipients" (Available [here](http://dev.fr8.co/dashboard/solution/Track_DocuSign_Recipients))
This solution monitors a user's DocuSign account, watching both for envelopes that the user sends out, and completion messages that are received
into the account as a result of action taken by the envelope recipients. 

In order to monitor this, this Fr8 Solution programmatically generates a second Plan consisting of a single Activity called Monitor DocuSign Envelope Activity ("MDEA"). It is considered an "internal" Plan and is not normally
visible to the user. 

FINISH THIS
NEED CLARITY ON MADSE, INTERNAL PLANS
