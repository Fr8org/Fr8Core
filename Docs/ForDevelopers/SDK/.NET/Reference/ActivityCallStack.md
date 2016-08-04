# ActivityCallStack

Information about currently executing activities. Information is represented as the stack of [StackFrames]()

**Namespace**: Fr8.Infrastructure.Data.Manifests  
**Assembly**: Fr8Infrastructure.NET

## Inheritance Hierarchy
* IEnumerable\<StackFrame>
  * **ActivityCallStack**

## Properties
| Name                            |Description                                                                          |
|---------------------------------|------------------------------------------------------------------------------------ |
| Count | Gets the number of stack frames |
| TopFrame | Gets the topmost frame |

## Methods
| Name                            |Description                                                                          |
|---------------------------------|------------------------------------------------------------------------------------ |
| RemoveTopFrame() | Pop the frame from the stack
| PushFrame(StackFrame) | Push the frame |
| Clear() | Remove all stack frames |
| StoreLocalData(string, object) | Sets stack-local data of the given type for the top frame |
| GetLocalData<T>(string) | Gets stack-local data from the top frame. To successfully retrieve the data you should specify the same **type** parameter that was used for setting the data. |



## Remarks
When **AcitvityCallStack** is being serialized to JSON it is represented as an array with the following order of the stack frames:
* The bottom element is stored at the beginning of the array. 
* The top element is stored in the end of the array.