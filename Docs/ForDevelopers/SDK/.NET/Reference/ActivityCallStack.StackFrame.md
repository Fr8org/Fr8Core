# StackFrame

Information about executing node. Node can be either Activity or Subplan.  

**Namespace**: Fr8.Infrastructure.Data.Manifests.ActivityCallStack  
**Assembly**: Fr8Infrastructure.NET

## Properties
| Name                            |Description                                                                          |
|---------------------------------|------------------------------------------------------------------------------------ |
| NodeId | Gets or sets identifier of the node |
| NodeName| Gets or sets the name of the node. This information is serving for debugging reasons only. |
| ActivityExecutionPhase | Gets or sets current node execution phase. See [Implementation details]() for more info.|
| CurrentChildId | If current node has children this property gets or sets currently executing child node Id. If the is no children this property has **null** value. |
| [LocalData]() | Stack-local data for the current node |

