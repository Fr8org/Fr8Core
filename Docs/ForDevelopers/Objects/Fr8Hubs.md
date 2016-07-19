# FR8 HUBS

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md) 

A Fr8 Hub is a web service that stores [Plans](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Plans.md) and processes [Containers](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Containers.md)  of Fr8. The Hub passes the Container to appropriate [Terminals](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Terminals.md)  so that the right Activity in the Plan can be executed.

Fr8 Hubs are also responsible for managing user accounts and authorization tokens.

The Fr8 Company operates a Fr8 Hub at fr8.co. However, any web service can be a Fr8 Hub if it supports the Fr8 standard API endpoints used by Clients and Terminals.

The operator of a Hub chooses which Terminals it wants to work with. A list of URLs is read at system startup from the root directory of the Hub. The Hub then makes an HTTP request to the Hub and receives back a list of support Activities.

Hub APIs
----------

Hub API's are available [via swagger](https://fr8.co/swagger/ui/index).

Note that not all Hub API's are callable by Terminals, for security reasons.

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  
