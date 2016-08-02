
[Return to Terminal Development Guide Home](/Docs/ForDevelopers/DevelopmentGuides/TerminalDevelopmentGuide.md)

### [Before you begin: Choosing a Development Approach](/Docs/ForDevelopers/DevelopmentGuides/ChoosingADevelopmentApproach.md)

### [Before you begin: Logging and Debugging](/Docs/ForDevelopers/DevelopmentGuides/BeforeYouBeginLogging.md)

### [Making Your Terminal Visible on the Public Web](/Docs/ForDevelopers/DevelopmentGuides/PublicVisibility.md)


### [Responding to /discover Requests](/Docs/ForDevelopers/DevelopmentGuides/Guide-TerminalDiscovery.md)


### [Registering your Terminal with a Hub](/Docs/ForDevelopers/DevelopmentGuides/Guide-TerminalRegistration.md)

### [Responding to /configure Requests](/Docs/ForDevelopers/DevelopmentGuides/Guide-ActivityConfiguration.md)

### [Communicating with Hub](/Docs/ForDevelopers/DevelopmentGuides/Guide-HubCommunication.md)

### Making the activity runnable

Activity execution is a bit more complex than configuration. There are three endpoints you have to implement to make activity ready for execution:

1. Activation endpoint: **/activities/activate**

2. Execution endpoint: **/activities/Run**

3. Deactivation endpoint: **/activities/deactivate**


### [Responding to /activate and /deactivate](/Docs/ForDevelopers/DevelopmentGuides/Guide-ActivateDeactivate.md)



### [Responding to /run](/Docs/ForDevelopers/DevelopmentGuides/Guide-ActivityExecution.md)

### [Getting deployed](/Docs/ForDevelopers/DevelopmentGuides/GettingDeployed.md)



More Information:

[Authentication](/Docs/ForDevelopers/OperatingConcepts/Authorization/Home.md)

[Validation](/Docs/ForDevelopers/OperatingConcepts/ActivitiesValidation.md)

[Plan Execution](/Docs/ForDevelopers/OperatingConcepts/PlanExecution.md)

[Plan Directory](/Docs/ForDevelopers/OperatingConcepts/PlanDirectory.md)

[Events](/Docs/ForDevelopers/OperatingConcepts/Events.md)

[Activity Signaling](/Docs/ForDevelopers/OperatingConcepts/Signaling.md)
