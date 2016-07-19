# IActivity

Defines an activity that can executed in context of .NET SDK for Terminals

**Namespace**: Fr8.TerminalBase.Interfaces  
**Assembly**: Fr8TerminalBase.NET


## Methods
| Name                            |Description                                                                                 |
|---------------------------------|------------------------------------------------------------------------------------------- |
| Run(ActivityContext, ContainerExecutionContext)  | Called when activity should run. This happens when **/activities/run** endpoint is triggered|
| RunChildActivities(ActivityContext, ContainerExecutionContext)  | Called when activity has children activities. This method is called after all children activties where processed by the Hub. This happens when **/activities/run** endpoint is triggered with **scope** request parameter is equals to **childActivities**. |
| Configure(ActivityContext)  | Called when activity should perform configuration logic. This happens when **/activities/configure** endpoint is triggered |
| Activate(ActivityContext)  | Called when activity should perform activation logic. This happens when **/activities/activate** endpoint is triggered |
| Deactivate(ActivityContext)  | Called when activity should perform activation logic. This happens when **/activities/deactivate** endpoint is triggered |
| GetDocumentation(ActivityContext, string)  | Called when activity should generate documentation. This happens when **/activities/documentation** endpoint is triggered |

## Remarks

You can implement this interface manually. But in general it is recommended to use several available base classes as the starting point of developing your activity:
* [TerminalActivity\<TUi>](/Docs/ForDevelopers/SDK/.NET/Reference/TerminalActivityT.md) (recommended for new activity development)
* [ExplicitTerminalActivity](/Docs/ForDevelopers/SDK/.NET/Reference/ExplicitTerminalActivity.md)
* [TerminalActivityBase](/Docs/ForDevelopers/SDK/.NET/Reference/TerminalActivityBase.md)

