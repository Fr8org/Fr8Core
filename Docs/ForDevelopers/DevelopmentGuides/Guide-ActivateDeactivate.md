
###  Terminal Development - Activation and Deactivation

The distinct Activation phase allows Plans to carry out Validation checks even if the Plan is going to passively monitor for a trigger event (as opposed to immediately being run).

If activation succeed, the Hub will either mark the Plan as a Running Plan (if it starts with a Monitor Activity) or execute the Activity (all other cases).

When user stops a "Running Plan" they may be either deactivating a non-executing Plan (if the Plan starts with a Monitor Activity and has not received a trigger and started execution) or terminating the execution of a Plan that's in the middle of execution (all other cases)
In either case, the system will call your Terminal's /deactivate endpoint for each Activity in the Plan.


Delailed infomration about activation and deactivation you can find [here](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/PlansActivationAndRunning.md). 

If actvity has no activation and deactivation logic you can simply return the Activity JSON without any changes. However, it's highly recommended that you provide Validation support. ([Learn about Fr8's Validation Services](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/ActivitiesValidation.md)). 

#### Activation

When user of our activtiy will trigger related plan exectuion the Hub will send POST HTTP request to **/activities/activate** endpoint
	
	http://terminal.com/activities/activate
    
The body and the headers of the request are absolutely identical to the body and headers that Hub sends with **/activities/configure** request. The format of the response is also identical to the response your activity returns to **/activities/configure** request. General rules of processing of this request are the same as for **follow-up configuration** request processing. It means that by the default, you return the same data that was send to you. You would never return empty response for the *activation* request. 

You may notice that *activation* is not triggered every time you run the plan, in order to save resources. Read more about the reasons of such behavior [here](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/PlansActivationAndRunning.md).

#### Deactivation
In our sample **/activities/deactivate** is likely to be called only when user run the plan than change our sample activity configuration and then run the plan again. *Deactivation* will be triggered just before the first call to **/activties/configure**. *Deactivation* endpoint signature, corresponding request/response and processing rules are identical to **/activities/activate**. Our activity needs no uninitialization so just implement processing of **/activities/deactivate** in the same way you done it for **/activities/activate**.
