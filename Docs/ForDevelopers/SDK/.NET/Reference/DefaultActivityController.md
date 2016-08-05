# DefaultActivityController

Base class for Web API controller that are intended to process activity related requests from the Hub. 

**Namespace**: Fr8.TerminalBase.BaseClasses  
**Assembly**: Fr8TerminalBase.NET


## Remarks

You have to create one controller that is derived from this class when developing terminals using .NET SDK. This class has no Fr8 specific methods to override. You can override available method from ApiControler, but in general you have not to do this. **DefaultActivityController** performs all necessary work to successfully process activity related requests from Hub. 

Typical usage:
```C#
	public class ActivityController : DefaultActivityController
    {
        public ActivityController(IActivityExecutor activityExecutor)
            : base(activityExecutor)
        {
        }
    }
```
