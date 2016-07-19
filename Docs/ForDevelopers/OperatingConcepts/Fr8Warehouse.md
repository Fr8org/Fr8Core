Fr8 Warehouse

The Fr8 Warehouse is a storage system that can store any crate that has a registered manifest. Terminals can store data in the Warehouse and retrieve it.
Similar to warehouses found in port facilities, the Fr8 Warehouse isn't intended to be a permanent long term archive of anything, however.

The  Warehouse is implemented as a multi-tenant database, patterned after the [Salesforce.com main database](https://developer.salesforce.com/page/Multi_Tenant_Architecture). It is currently
implemented as a set of 3 SQL tables, corresponding to Objects, Fields, and Data. However, Terminals will never deal with that level of the Warehouse, instead accessing it
through the [/warehouse endpoint on the Hub](https://fr8.co/swagger/ui/index#/Warehouse). 

As an example, the Monitor DocuSign 
