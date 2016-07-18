# IActivityFactory

Factory that is generating activity instances for the certain activity template.

**Namespace**: Fr8.TerminalBase.Interfaces  
**Assembly**: Fr8TerminalBase.NET


## Methods
| Name                            |Description                                                                                 |
|---------------------------------|------------------------------------------------------------------------------------------- |
| Create (IContainer)   | Creates instance of the activity using supplied DI container to resolve dependencies|


## Remarks

Container that is passes to **Crate** method is request-specific container (see [Dependency injection and DI container configuration](/Docs/ForDevelopers/SDK/.NET/DI%20container%20configuration.md) topic for details). It means that each activity can take advantage of request specific services.