# Plan execution

Activities execution in Fr8 is stack-based and information related to the stack is stored not in the Hub but in the JSON data of the Payload Container itself. This provides Payload Containers with considerable portability. A Container can be moved from one Hub to another Hub, and the receiving Hub will know exactly where to execute the new Activity. It also enables asynchronicity. An Activity can pause processing of a Payload Container, wait for some real world activity (such as a person responding to an email or sms), and then post an event to the Hub to resume processing, and the Hub will be able to look in the Payload Container and know where processing paused and where to resume.

Stack-related data and other data related to run-time execution of a Plan is stored, like almost all Fr8 data, in a Crate. Here, the special crate is called the Operation State Crate, and it uses the **OperationalStateCM** manifest. 

Operational State Crate
=====================

This manifest is designed to control all main aspects of the container execution:
1. It stores activity *Call Stack*. 
2. It stores activity's *Stack-Local data*
3. It is used for transferring [Response](/Docs/ForDevelopers/Objects/Activities/ActivityResponses.md) data from the activity that is currently being executed.  
4. It stores the container execution *History*. Execution History is the chain of containers that have preceded execution of the current one. If the container execution is initiated by the user directly then History remains empty.

Usage Notes
------------------------
Since multiple parties, including the Hub, use the same Operational State crate to store data, be careful about making assumptions about what you'll find there. 

The  **OperationalStateCM** Crate is created when the Container is created. You should avoid creating additional ones. 

> **Important!**  
>The only thing that can be changed by the activity is **LocalData** property of the stack frame which is related to this activity. All other changes will be discarded by the Hub.

See [Implementation details](/Docs/ForDevelopers/SDK/.NET/ContainerExecutionImpl.md) for in-depth explanations of container execution logic. 

Operational State Crate Element: Current ActivityResponse
-----------------------------------------------------------
A special Fr8-specific data structure used by Activities to provide status with their responses to Hub requests. [Learn more.](/Docs/ForDevelopers/Objects/Activities/ActivityResponses.md)




# The Call Stack
These rules govern the Call Stack:
1. During Activity Execution, the Hub pushes a Stack Frame of information about the activity to the Call Stack.
2. A Stack Frame is popped from the stack when execution of an Activity finishes, in one of the following cases:
   1. The completed Activity has no children.
   2. The completed Activity has children, and they have all been executed. 
   3. The completed Activity returned the ActivityResponses Jump To Activity or Jump to Subplan. Before jumping to the specified activity or subplan current activity is removed from the call stack even if it has children.

As would be expected, an ActivityResponse of Request Suspend does not pop the stack, so that the returning Activity will be called again, when execution resumes. 

Here are a few examples of **OperationalStateCM** content during the different steps of the container execution. 

### Linear sequence of activities
Here is our plan:  
![Linear sequence of activities](/Docs/img/PlanExecution-LinearSequenceOfActivities.png)

And this is logical steps of the  container execution for this plan:  
![Execution sequence](/Docs/img/PlanExecution-LinearExecutionSequence.png)

#### Step 1
When container is created the Hub pushes the first node of the plan to the stack so stack is already not empty. The first node is usualy a starting subplan of the plan. Not the activity:
```JavaScript
{  
   "CallStack":[  
      {  
         "NodeId":"4554d028-9955-4121-97e0-2fb9a1e40e80",
         "NodeName":"Starting subplan",
         "CurrentActivityExecutionPhase":0,
         "CurrentChildId":null,
         "LocalData":null
      }
   ],
   "History":[  

   ],
   "CurrentActivityResponse":null
}
```

Note the **NodeName** property. The Hub will initialize this property with the corresponding node name. The main reason of this property is to simplify debugging.

#### Step 2
Actually step 2, 4, 6 are very similar to each other. At these steps the Hub tries to execute the node at the top of the stack frame (if this node can be executed) and basing in response(if any) Hub pushes the next node to the stack. At the step 2 node at the top of the stack is Subplan. Subplan itself can't be executed. So the hub will select the first descendant of the subplan that is Add Payload Activity:

``` JavaScript
{
   "CallStack":[
      {
         "NodeId":"4554d028-9955-4121-97e0-2fb9a1e40e80",
         "NodeName":"Starting subplan",
         "CurrentActivityExecutionPhase":1,
         "CurrentChildId":"8c3f4b59-a169-408e-a0aa-9b01c6faf092",
         "LocalData":null
      },
      {
         "NodeId":"8c3f4b59-a169-408e-a0aa-9b01c6faf092",
         "NodeName":"Activity: Add Payload Manually",
         "CurrentActivityExecutionPhase":0,
         "CurrentChildId":null,
         "LocalData":null
      }
   ],
   "History":[

   ],
   "CurrentActivityResponse":null
}
```
Note that subplan node was not remove from the stack, because there are children being executed. In general plan execution is resembling a usual call stack used in many programming laguages. 

#### Step 3
At this step the Hub calls the terminal **/run** endpoint and updates OperationalStateCM with activity response data:
``` JavaScript
{
   "CallStack":[
      {
         "NodeId":"4554d028-9955-4121-97e0-2fb9a1e40e80",
         "NodeName":"Starting subplan",
         "CurrentActivityExecutionPhase":1,
         "CurrentChildId":"8c3f4b59-a169-408e-a0aa-9b01c6faf092",
         "LocalData":null
      },
      {
         "NodeId":"8c3f4b59-a169-408e-a0aa-9b01c6faf092",
         "NodeName":"Activity: Add Payload Manually",
         "CurrentActivityExecutionPhase":0,
         "CurrentChildId":null,
         "LocalData":null
      }
   ],
   "History":[

   ],
   "CurrentActivityResponse":{
      "type":"Success",
      "body":null
   }
}
```  
Note that **CurrentActivityResponse** property is not empty now. We can see that **Add Payload Manually** has returned **Success**.

#### Step 4
At this step hub will analyze current activity response and choose the next node to run. If no specific instructions were  given within activity response and there is no children the Hub will remove the top stack frame and push the next sibling of the current activity:
``` JavaScript
{  
   "CallStack":[  
      {  
         "NodeId":"4554d028-9955-4121-97e0-2fb9a1e40e80",
         "NodeName":"Starting subplan",
         "CurrentActivityExecutionPhase":1,
         "CurrentChildId":"68f33ea1-fdb0-4e55-a3f5-e4e59dc69520",
         "LocalData":null
      },
      {  
         "NodeId":"68f33ea1-fdb0-4e55-a3f5-e4e59dc69520",
         "NodeName":"Activity: Publish To Slack",
         "CurrentActivityExecutionPhase":0,
         "CurrentChildId":null,
         "LocalData":null
      }
   ],
   "History":[  

   ],
   "CurrentActivityResponse":null
}
```
Note that activity response information has been resetted by the Hub.

#### Step 5
Is identical with the **Step 3**
``` JavaScript
{
   "CallStack":[
      {
         "NodeId":"4554d028-9955-4121-97e0-2fb9a1e40e80",
         "NodeName":"Starting subplan",
         "CurrentActivityExecutionPhase":1,
         "CurrentChildId":"68f33ea1-fdb0-4e55-a3f5-e4e59dc69520",
         "LocalData":null
      },
      {
         "NodeId":"68f33ea1-fdb0-4e55-a3f5-e4e59dc69520",
         "NodeName":"Activity: Publish To Slack",
         "CurrentActivityExecutionPhase":0,
         "CurrentChildId":null,
         "LocalData":null
      }
   ],
   "History":[

   ],
   "CurrentActivityResponse":{
      "type":"Success",
      "body":null
   }
}
```

#### Step 6
At this step call stack is starting to collapse. After **Publish to Slack** activity the Hub will remove information from the call stack, and as this is the last activity in the plan the Hub will not add new stack frames:
```JavaScript
{
   "CallStack":[
      {
         "NodeId":"4554d028-9955-4121-97e0-2fb9a1e40e80",
         "NodeName":"Starting subplan",
         "CurrentActivityExecutionPhase":1,
         "CurrentChildId":"68f33ea1-fdb0-4e55-a3f5-e4e59dc69520",
         "LocalData":null
      }
   ],
   "History":[

   ],
   "CurrentActivityResponse":null
}
```
#### Stopping the execution
After **Step 6** the Hub will see that it has processed all children of the starting subplan. Hub will remove the last stack frame related to the subplan from the stack. Hub is programmed not to continue execution of subplan siblings. So call stack becomes empty indicating that execution has finished.
 

## Stack-Local data
Activity can store custom data within the stack frame by setting **LocalData** property of the stack frame. Stack-local data is persisted until corresponding stack frame is popped from the stack. There is one exception when stack-local data can survive popping of the stack frame: if activity requests the Hub to jump to itself, stack-local data of this activity is persisted. Despite apparent uselessness because of limited stack frame lifetime this is especially useful for activities that has children.  

Lets see how **Loop** activity uses stack-local data to track the current iteration counter.

Here is the plan with Loop:  
![Execution sequence](/Docs/img/PlanExecution-PlanWithLoop.png)

And this is how **OperationalStateCM** looks like after the **Publish To Slack** has been executed for the first time:

```JavaScript
{
   "CallStack":[
      {
         "NodeId":"051ca542-f891-49c7-864d-99aaadb6394b",
         "NodeName":"Starting subplan",
         "CurrentActivityExecutionPhase":1,
         "CurrentChildId":"c61519f8-e2ec-4a22-bfa6-f7a56a32cddc",
         "LocalData":null
      },
      {
         "NodeId":"c61519f8-e2ec-4a22-bfa6-f7a56a32cddc",
         "NodeName":"Activity: Loop",
         "CurrentActivityExecutionPhase":1,
         "CurrentChildId":"15603dbe-4f2e-4b42-924d-6f2ac1272eb9",
         "LocalData":{
            "Type":"Loop",
            "Data":{
               "Index":0,
               "CrateManifest":{
                  "CrateDescriptions":[
                     {
                        "manifestId":9,
                        "manifestType":"Standard Table Data",
                        "label":"Table Generated From Google Sheet Data",
                        "producedBy":"Get_Google_Sheet_Data",
                        "selected":true,
                        "availability":2,
                        "fields":[
                            .....
							Information about fields was removed for clarity
							.....
                        ]
                     }
                  ]
               }
            }
         }
      },
      {
         "NodeId":"15603dbe-4f2e-4b42-924d-6f2ac1272eb9",
         "NodeName":"Activity: Publish To Slack",
         "CurrentActivityExecutionPhase":1,
         "CurrentChildId":null,
         "LocalData":null
      }
   ],
   "History":[

   ],
   "CurrentActivityResponse":{
      "type":"Success",
      "body":null
   }
}
```

You may noticed that **LocalData** for the stack frame which is related to **Loop** activity is not empty. After it was  executed, **Loop** activity stored current iteration id - **Index**  and the crate, that is used to iterate over - **CrateManifest** as stack-local data. On each subsequent iteration **Loop** will increase **Index** property by 1 until **Standard Table Data** has rows to process.
