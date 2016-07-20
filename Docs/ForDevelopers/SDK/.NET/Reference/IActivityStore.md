# IActivityStore

Service that stores and manages information about the current terminal and registered activities

Service is registered as a singleton within the DI container. This service is available globally.

**Namespace**: Fr8.TerminalBase.Services  
**Assembly**: Fr8TerminalBase.NET

## Properties
| Name                            |Type                     |Description                                                                                 |
|---------------------------------|-------------------------|------------------------------------------------------------------------------------------- |
| Terminal					      | TerminalDTO             |Information about the current terminal                                                      |


## Methods
| Name                            |Description                                                                                 |
|---------------------------------|------------------------------------------------------------------------------------------- |
| RegisterActivity (ActivityTemplateDTO, IActivityFactory)   | Register activity template with user supplied activity factory|
| RegisterActivity\<T> (ActivityTemplateDTO)   | Register activity template with default activity factory|
| GetFactory(ActivityTemplateDTO)   | Gets activity factory that can be used to create instances of activities for supplied activity template|
| GetAllTemplates () | Gets list of all activity templates that are registered within the current terminal |

## Remarks

Default activity factory that is used by this services allows you to take advantage of constructor injection in Activity classes. This is the recommended way to resolve dependencies. 

For example, Set_Delay_v1 needs **ICrateManager** service to power its business logic. You can write the following constructor:

```C#
public Set_Delay_v1(ICrateManager crateManager)
            : base(crateManager)
{
}
```

and instance of ICrateManager will be injected into activity constructor by .NET SDK for Terminals.