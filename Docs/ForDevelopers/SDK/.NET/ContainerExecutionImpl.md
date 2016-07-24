# Container execution logic implementation details

To illustrate the general concept, here is the simplified code that recursively executes all activities within some parent:
```C#
public static void Run(Activity activity)
 
{
	// phase #1
	activity.Run("Run");

	// phase #2
	foreach (var child in activity.Children)
	{
		Run(child);
	}

	// phase #3
	if (activity.Children.Count > 0)
	{
		activity.Run("Run After All Children Executed");
	}
}
```

Code above is very simple, but it illustrate most important **phases** of activity execution:

1.  initial Run
2.  processing activity's children
3.  calling activity's special method after all children are executed.


In fr8 we should consider the following:
At any time our execution can be interrupted and we want to be able to resume execution flawlessly
To allow for execution to be suspended and resume, Fr8 uses a  stack of StackFrame objects:

```C#
public class StackFrame
{
	public Guid NodeId { get; set; } // node that is 
	public string NodeName { get; set; }
	public ActivityExecutionPhase CurrentActivityExecutionPhase { get; set; }
	public Guid? CurrentChildId { get; set; }
}
```

Each StackFrame contains all information we have during each call to 'Run' in our example:
* Current activity (**NodeId** and **NodeName**)
* Current 'child' in foreach (var child in activity.Children) loop.
* Phase of execution  (**CurrentActivityExecutionMode**)
* The **StackFrame** tracks the current child because container execution can be interrupted while we are iterating through child activities 

And we have to have a way to resume execution from the place we were interrupted. Phase of execution is serving the same reason: it is used to better determine where we were interrupted not to duplicate actions has been done before interruption.

Execution Phases
----------------

Phase is represented as **CurrentActivityExecutionPhase** property inside the **StackFrame**. **ActivityExecutionPhase** is a enum that currently has  two defined values:
 * **WasNotExecuted (0)** -  (we have not call **Run** for the current activity yet)
 * **ProcessingChildren (1)** -  This is used both for **phase 2** and **phase 3**.  
 This is possible because we can easily distinguish **phase 2** from **phase 3** by looking at **CurrentChildId**.
 If we have no children to execute than we are at **phase 3** otherwise and **phase 2**.
 
Execution flow
--------------

Almost all logic is implemented in ExecutionSession class. Logic is split in two main methods:  
* **Run** - it is stack-based implementation of the code in the beginning of this document + some exception handling and fr8 specific features.   
* **ProcessOpCode** - a method that executes some logic that affect natural flow execution based on response from the activity (like **JumpToSubplan**, **JumpToActivity** and so on). See [ActitivityResponse page](https://github.com/Fr8org/Fr8Core/blob/docs8/Docs/ForDevelopers/Objects/Activities/ActivityResponses.md) for more info about possible responses and their meaning. 

Natural execution flow is the following:

1. Read call stack from the payload into the memory
2. Look at the top of the stack
3. If top frame's **CurrentActivityExecutionPhase**  is **ExecutingChildren** go to step 9
4. If this is an activity we execute **Run** and read response, otherwise go to step 8
5. Replace all container's payload storage with crates from the response 
6. Find [OperationalStateCM](/Docs/ForDevelopers/SDK/.NET/Reference/OperationalStateCM.md) crate and replace call stack with the call stack we store in memory (to avoid stack corruption by the activity)
7. Overwrite **LocalData** for each in-memory stack frame with corresponding **LocalData** from activity response's stack frame (because activity can persist run-time data here). We need this stuff for the current implementation of Loop activity.
8. Change top stack frame's **CurrentActivityExecutionPhase** to **ExecutingChildren**
9. Look at the top stack frame's **CurrentChildId**. 
10. If it is null set **CurrentChildId** to the first child if any