# Plans
[Go to Contents](/Docs/Home.md)  

A Fr8 Plan is a JSON element that defines a series of Activities. It may have one or more Subplans. When a Plan is run, a Fr8 Hubs first generates a Payload Container, which is a JSON structure designed to store any data generated during the execution of the Plan. The Hub then identifies the Terminal responsible for the starting Activity, and posts the Payload Container off to it. 

When a Terminal has completed the processing of an Activity, it returns the Payload Container (which it has probably modified in some way) to the Hub, and the Hub moves on to the next Activity.

[Watch Video Introduction to Plans](https://vimeo.com/173975037)


Creating Plans
--------------

Plans can be created in several ways:
1) You can build them by hand in the [Plan Builder].
2) You can build them programmatically, using a Terminal.
3) You can upload the Plan Template created when someone downloaded a Plan into a local JSON text file.
4) You can select a Plan from the many listed in the Plan Directory


If you're developing a Terminal, you don't really worry about Plans. You're mostly focused on individual Activities. The fact that a Terminal developer doesn't need to pay attention to the other Activities in the Plan is one of Fr8's great strengths.

Flow Control
------------
Fr8 provides a powerful set of [flow control](/Docs/ForDevelopers/Objects/Activities/ActivityResponses.md) tools that can be integrated into Plans.




Additional Resources
--------------------
[The Plan JSON Definition](/Docs/ForDevelopers/Objects/Plans/PlanJSONDefinition.md)  
[Activating & Running Plans](/Docs/ForDevelopers/Objects/PlansActivationAndRunning.md)  
[Moving and Sharing Plans](/Docs/ForDevelopers/Objects/Plans/MovingPlans.md)

[Plan Execution](/Docs/ForDevelopers/OperatingConcepts/PlanExecution.md)

[Go to Home](/Docs/Home.md)  
