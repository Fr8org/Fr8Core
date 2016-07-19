# IHubDiscoveryService

Service that allows to gain access to multiple Hubs in those where the current terminal is registered.

Service is registered as a singleton within the DI container. This service is available globally.

**Namespace**: Fr8.TerminalBase.Services  
**Assembly**: Fr8TerminalBase.NET


## Methods
| Name                            |Description                                                                                 |
|---------------------------------|------------------------------------------------------------------------------------------- |
| GetHubCommunicator(string)   | Gets the instance of IHubCommunicator that can be used to work with the hub at the given URL.|
| GetMasterHubCommunicator()   | Gets the instance of IHubCommunicator that can be used to work with the Master hub for this terminal.|
| SetHubSecret(string, string) | Set actual secret to communicate with the Hub at the given URL. This method is unlikely to be used by the user's code.|
| GetSubscribedHubs() | Gets a list of Hubs that current terminal is working with.|


## Remarks

See [Termnal Discovery](/Docs/ForDevelopers/SDK/.NET/TerminalDiscovery.md) for more information about how terminal and Hub get know of each other. 

When your code needs to perform some operations with Hub outside of Activity code you should use this service to get IHubCommunicator that can be used to work with the Hub you are interested in. One possible example of such situation is when you are processing request from the external service, like Slack or Docusign notification. 
